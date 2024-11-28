using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using ConcurrentCollections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.GZip;
using Vint.Core.Config.JsonConverters;
using Vint.Core.Config.MapInformation;
using Vint.Core.Discord;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Server.Common;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Vint.Core.Config;

public static class ConfigManager {
    public static Func<string, long, Task<bool>>? NewLinkRequest { get; set; }
    public static ConcurrentHashSet<DiscordLinkRequest> DiscordLinkRequests { get; } = [];

    public static ServerConfig ServerConfig { get; private set; } = null!;
    public static FrozenSet<MapInfo> MapInfos { get; private set; } = null!;
    public static FrozenDictionary<string, BlueprintChest> Blueprints { get; private set; } = null!;
    public static FrozenDictionary<string, Regex> CensorshipRegexes { get; private set; } = null!;
    public static ModulePrices ModulePrices { get; private set; }
    public static DiscordConfig Discord { get; private set; }
    public static QuestsInfo QuestsInfo { get; private set; }
    public static CommonMapInfo CommonMapInfo { get; private set; }

    public static IEnumerable<string> GlobalEntitiesTypeNames => Root
        .Children
        .Where(child => child.Value.Entities.Count != 0)
        .Select(child => child.Key);

    static ILogger Logger { get; } = Log.Logger.ForType(typeof(ConfigManager));
    public static string ResourcesPath { get; } = Path.Combine(Directory.GetCurrentDirectory(), "Resources");

    static FrozenDictionary<string, byte[]> LocaleToConfigCache { get; set; } = FrozenDictionary<string, byte[]>.Empty;
    static ConfigNode Root { get; } = new();

    public static async Task InitializeChatCensorship() {
        if (!ChatUtils.CensorshipEnabled) return;

        Logger.Information("Initializing chat censorship");

        string rootPath = Path.Combine(ResourcesPath, "ChatCensorship");
        string replacementsPath = Path.Combine(rootPath, "Replacements");
        string badWordsPath = Path.Combine(rootPath, "badwords.txt");

        ConcurrentDictionary<char, string> replacements = new(Directory
            .EnumerateFiles(replacementsPath, "*.json", SearchOption.TopDirectoryOnly)
            .Select(replacementPath => JsonConvert.DeserializeObject<Dictionary<char, string>>(File.ReadAllText(replacementPath))!)
            .Aggregate(new Dictionary<char, string>(),
                (current, stringToRegex) => current
                    .Concat(stringToRegex)
                    .ToDictionary()));

        string[] badWords = await File.ReadAllLinesAsync(badWordsPath);
        ConcurrentDictionary<string, Regex> regexes = new();

        Parallel.ForEach(badWords,
            word => {
                Logger.Debug("Preparing {Word}", word);

                StringBuilder patternBuilder = new();

                foreach (char @char in word) {
                    if (!replacements.TryGetValue(@char, out string? pattern))
                        patternBuilder.Append(@char);
                    else patternBuilder.Append(pattern);
                }

                Regex regex = new(patternBuilder.ToString(), RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

                regexes.TryAdd(word, regex);
                Logger.Verbose("{Word}: {Regex}", word, regex);
            });

        CensorshipRegexes = regexes.ToFrozenDictionary();
        Logger.Information("Chat censorship initialized");
    }

    public static async Task InitializeMapInfos() {
        Logger.Information("Parsing map infos");

        string mapsPath = Path.Combine(ResourcesPath, "Maps");
        string commonMapInfoPath = Path.Combine(mapsPath, "common.json");

        CommonMapInfo = JsonConvert.DeserializeObject<CommonMapInfo>(await File.ReadAllTextAsync(commonMapInfoPath));

        HashSet<MapInfo> mapInfos = [];

        foreach (string map in Directory.EnumerateDirectories(mapsPath)) {
            string mapName = Path.GetFileName(map);
            string mapInfoPath = Path.Combine(map, "info.json");

            MapInfo mapInfo = JsonConvert.DeserializeObject<MapInfo>(await File.ReadAllTextAsync(mapInfoPath));
            mapInfo.Name = mapName;

            mapInfo.Init();

            mapInfos.Add(mapInfo);
        }

        MapInfos = mapInfos.ToFrozenSet();
        Logger.Information("Map infos parsed");
    }

    public static void InitializeCache() {
        Logger.Information("Generating config archives");

        string rootPath = Path.Combine(ResourcesPath, "Configuration");
        string configsPath = Path.Combine(rootPath, "configs");
        string localizationsPath = Path.Combine(rootPath, "localization");

        Dictionary<string, byte[]> localeToConfigCache = new(2);

        foreach (string localeDir in Directory.EnumerateDirectories(localizationsPath)) {
            string locale = new DirectoryInfo(localeDir).Name;

            Logger.Debug("Generating archive for the '{Locale}' locale", locale);

            using MemoryStream outStream = new();

            using (IWriter writer = WriterFactory.Open(outStream, ArchiveType.Tar, new GZipWriterOptions())) {
                writer.WriteAll(configsPath, "*", SearchOption.AllDirectories);
                writer.WriteAll(localeDir, "*", SearchOption.AllDirectories);
            }

            byte[] buffer = outStream.ToArray();
            localeToConfigCache[locale] = buffer;
        }

        LocaleToConfigCache = localeToConfigCache.ToFrozenDictionary();
        Logger.Information("Cache for config archives generated");
    }

    public static async Task InitializeNodes() {
        Logger.Information("Generating config nodes");

        string configsPath = Path.Combine(ResourcesPath, "Configuration", "configs");

        IDeserializer deserializer = new DeserializerBuilder()
            .WithNodeDeserializer(new ComponentDeserializer())
            .WithNodeTypeResolver(new ComponentDeserializer())
            .WithTypeConverter(new Vector3TypeConverter())
            .IgnoreUnmatchedProperties()
            .Build();

        foreach (string filePath in Directory
                     .EnumerateFiles(configsPath, "*.*", SearchOption.AllDirectories)
                     .OrderBy(x => x)) {
            string relativePath = Path
                .GetRelativePath(configsPath, filePath)
                .Replace('\\', '/');

            string fileName = Path.GetFileName(filePath);

            if (string.IsNullOrEmpty(fileName)) continue;

            Logger.Verbose("Parsing {File}", relativePath);

            Dictionary<string, List<IComponent>> components = new();
            Dictionary<string, long?> ids = new();

            switch (fileName) {
                case "id.yml": {
                    Dictionary<string, long> obj = new Deserializer().Deserialize<Dictionary<string, long>>(await File.ReadAllTextAsync(filePath));

                    ids[relativePath[..^7]] = obj["id"];
                    break;
                }

                case "public.yml": {
                    if (deserializer.Deserialize(await File.ReadAllTextAsync(filePath)) is Dictionary<object, object> dict)
                        components[relativePath[..^11]] = dict
                            .Values
                            .OfType<IComponent>()
                            .ToList();

                    break;
                }
            }

            foreach ((string key, List<IComponent> comps) in components) {
                ConfigNode curNode = Root;

                foreach (string part in key.Split('/')) {
                    if (curNode.Children.TryGetValue(part, out ConfigNode child))
                        curNode = child;
                    else {
                        ids.TryGetValue(key, out long? id);
                        curNode = curNode.Children[part] = new ConfigNode { Id = id };
                    }
                }

                foreach (IComponent component in comps) {
                    Type componentType = component.GetType();

                    if (componentType.IsDefined(typeof(ProtocolIdAttribute)))
                        curNode.Components[componentType] = component;
                    else
                        curNode.ServerComponents[componentType] = component;
                }

                foreach (IComponent serverComponent in curNode.ServerComponents.Values) {
                    foreach (Type type in serverComponent
                                 .GetType()
                                 .FindInterfaces((type, iType) => type.IsGenericType && ReferenceEquals(type.GetGenericTypeDefinition(), iType),
                                     typeof(IConvertible<>))) {
                        Type resultType = type.GenericTypeArguments[0];

                        curNode.Components.TryGetValue(resultType, out IComponent? component);
                        component ??= (IComponent)RuntimeHelpers.GetUninitializedObject(resultType);

                        type.GetMethod("Convert")!.Invoke(serverComponent, [component]);
                        curNode.Components.TryAdd(resultType, component);
                    }
                }
            }
        }

        Logger.Information("Config nodes generated");
    }

    public static async Task InitializeGlobalEntities() {
        Logger.Information("Generating global entities");

        List<Type> types = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .ToList();

        string rootPath = Path.Combine(ResourcesPath, "GlobalEntities");

        Dictionary<string, Dictionary<string, IEntity>> globalEntities = new();

        List<string> typesToLoad =
            JsonConvert.DeserializeObject<List<string>>(await File.ReadAllTextAsync(Path.Combine(rootPath, "typesToLoad.json")))!;

        foreach (string filePath in typesToLoad.Select(type => Path.Combine(rootPath, $"{type}.json"))) {
            string relativePath = Path
                .GetRelativePath(rootPath, filePath)
                .Replace('\\', '/');

            string entitiesTypeName = Path.GetFileNameWithoutExtension(filePath);

            Logger.Verbose("Parsing {File}", relativePath);

            JArray jArray = JArray.Parse(await File.ReadAllTextAsync(filePath));

            Dictionary<string, IEntity> entities = new(jArray.Count);

            foreach (JToken jToken in jArray) {
                string entityName = jToken["name"]!.ToObject<string>()!;

                Logger.Verbose("Generating '{Name}'", entityName);

                long entityId = jToken["id"]!.ToObject<long>();

                if (entityId == 0)
                    entityId = EntityRegistry.GenerateId();

                JArray templateComponents = jToken["template"]!.ToObject<JArray>()!;

                string templateName = templateComponents[0]
                    .ToObject<string>()!;

                string configPath = templateComponents[1]
                    .ToObject<string>()!;

                JObject rawComponents = jToken["components"]!.ToObject<JObject>()!;

                List<IComponent> components = new(rawComponents.Count);

                foreach ((string rawComponentName, JToken? rawComponentProperties) in rawComponents) {
                    Type componentType = types.Single(type => type.Name == rawComponentName);

                    ConstructorInfo componentCtor = componentType
                        .GetConstructors()
                        .First();

                    ParameterInfo[] ctorParameters = componentCtor.GetParameters();

                    object?[] parameters = ctorParameters
                        .Select(ctorParameter => {
                            JToken? rawComponentProperty = rawComponentProperties![ctorParameter.Name!];

                            if (rawComponentProperty == null &&
                                ctorParameter.HasDefaultValue)
                                return ctorParameter.DefaultValue;

                            return rawComponentProperty?.ToObject(ctorParameter.ParameterType);
                        })
                        .ToArray();

                    IComponent component = componentType.IsAssignableTo(typeof(GroupComponent))
                        ? GroupComponentRegistry.FindOrCreateGroup(componentType, (long)parameters.Single()!)
                        : (IComponent)componentCtor.Invoke(parameters);

                    components.Add(component);
                }

                Type templateType = types.Single(type => type.Name == templateName);

                ConstructorInfo templateCtor = templateType
                    .GetConstructors()
                    .First();

                EntityTemplate template = (EntityTemplate)templateCtor.Invoke(null);

                IEntityBuilder entityBuilder = new EntityBuilder(entityId).WithTemplateAccessor(template, configPath);
                components.ForEach(component => entityBuilder.AddComponent(component));

                IEntity entity = entityBuilder.Build(false);

                entities[entityName] = entity;

                Logger.Verbose("Generated {Entity}", entity);
            }

            globalEntities[entitiesTypeName] = entities;
        }

        Logger.Debug("Generating nodes for global entities");

        foreach ((string entitiesTypeName, Dictionary<string, IEntity> entities) in globalEntities) {
            ConfigNode curNode = Root;

            if (curNode.Children.TryGetValue(entitiesTypeName, out ConfigNode child))
                curNode = child;
            else
                curNode = curNode.Children[entitiesTypeName] = new ConfigNode();

            foreach ((string entityName, IEntity entity) in entities)
                curNode.Entities[entityName] = entity;
        }

        Logger.Information("Global entities generated");
    }

    public static async Task InitializeConfigs() {
        Logger.Information("Initializing configs");

        ServerConfig = JsonConvert.DeserializeObject<ServerConfig>(await File.ReadAllTextAsync(ServerConfig.FilePath))!;
        Discord = JsonConvert.DeserializeObject<DiscordConfig>(await File.ReadAllTextAsync(Path.Combine(ResourcesPath, "discord.json")));
        ModulePrices = JsonConvert.DeserializeObject<ModulePrices>(await File.ReadAllTextAsync(Path.Combine(ResourcesPath, "modulePrices.json")));

        QuestsInfo = JsonConvert.DeserializeObject<QuestsInfo>(await File.ReadAllTextAsync(Path.Combine(ResourcesPath, "quests.json")),
            new TimeOnlyConverter());

        Blueprints =
            JsonConvert.DeserializeObject<Dictionary<string, BlueprintChest>>(
                await File.ReadAllTextAsync(Path.Combine(ResourcesPath, "blueprints.json")))!.ToFrozenDictionary();

        Logger.Information("Configs initialized");
    }

    public static IEntity GetGlobalEntity(string path, string entityName) {
        ConfigNode node = GetNode(path)!.Value;

        return node
            .Entities[entityName]
            .Clone();
    }

    public static IEnumerable<IEntity> GetGlobalEntities(string path) {
        ConfigNode node = GetNode(path)!.Value;

        return node.Entities.Values.Select(entity => entity.Clone());
    }

    public static IEnumerable<IEntity> GetGlobalEntities() =>
        Root.Children.SelectMany(child => child.Value.Entities.Values.Select(entity => entity.Clone()));

    public static T GetComponent<T>(string path) where T : class, IComponent =>
        GetComponentOrNull<T>(path)!;

    public static T? GetComponentOrNull<T>(string path) where T : class, IComponent {
        ConfigNode? node = GetNode(path);

        if (!node.HasValue) return null;

        if (!node.Value.Components.TryGetValue(typeof(T), out IComponent? component))
            node.Value.ServerComponents.TryGetValue(typeof(T), out component);

        return component?.Clone() as T;
    }

    public static bool TryGetComponent<T>(string path, [NotNullWhen(true)] out T? component) where T : class, IComponent =>
        (component = GetComponentOrNull<T>(path)) != null;

    public static bool TryGetConfig(string locale, [NotNullWhen(true)] out byte[]? config) =>
        LocaleToConfigCache.TryGetValue(locale, out config);

    static ConfigNode? GetNode(string path) {
        ConfigNode curNode = Root;

        if (string.IsNullOrWhiteSpace(path)) return null;

        path = path.Replace('\\', '/');

        if (path.StartsWith('/')) path = path[1..];

        foreach (string part in path.Split('/'))
            if (curNode.Children.TryGetValue(part, out ConfigNode child))
                curNode = child;
            else return null;

        return curNode;
    }

    record struct ConfigNode() {
        public long? Id { get; init; }
        public Dictionary<Type, IComponent> Components { get; } = new();
        public Dictionary<Type, IComponent> ServerComponents { get; } = new();
        public Dictionary<string, IEntity> Entities { get; } = new();
        public Dictionary<string, ConfigNode> Children { get; } = new();
    }
}

// Copied from https://github.com/Assasans/TXServer-Public/blob/database/TXServer/Core/Configuration/ComponentDeserializer.cs
public partial class ComponentDeserializer : INodeTypeResolver, INodeDeserializer {
    ILogger Logger { get; } = Log.Logger.ForType<ComponentDeserializer>();
    Type? Type { get; set; }

    IEnumerable<Type> Types { get; } = Assembly
        .GetExecutingAssembly()
        .GetTypes();

    public bool Deserialize(
        IParser reader,
        Type expectedType,
        Func<IParser, Type, object?> nestedObjectDeserializer,
        out object? retValue,
        ObjectDeserializer rootDeserializer) {
        if (!typeof(IComponent).IsAssignableFrom(expectedType)) {
            retValue = null;
            return false;
        }

        IComponent component = (IComponent)(Activator.CreateInstance(expectedType) ?? RuntimeHelpers.GetUninitializedObject(expectedType));

        reader.MoveNext();

        while (reader.Current != null && reader.Current is not MappingEnd) {
            if (reader.Current is not Scalar scalar) continue;

            string key = scalar
                             .Value[0]
                             .ToString()
                             .ToUpper() +
                         scalar.Value[1..];

            PropertyInfo? info = expectedType.GetProperty(key);

            reader.MoveNext();

            if (info == null) {
                reader.SkipThisAndNestedEvents();
                continue;
            }

            object? value = nestedObjectDeserializer(reader, info.PropertyType);
            info.SetValue(component, value);

            Logger.Verbose(">> {Key}: {Value}", key, value);
        }

        reader.MoveNext();

        retValue = component;
        return true;
    }

    public bool Resolve(NodeEvent? nodeEvent, ref Type currentType) {
        if (Type != null) {
            Type type = Type;
            Type = null;

            if (nodeEvent is not MappingStart)
                return false;

            Logger.Debug("> {Type}", type);
            currentType = type;
            return true;
        }

        if (nodeEvent is not Scalar scalar ||
            scalar.Value.Length < 2 ||
            !MyRegex()
                .IsMatch(scalar.Value)) return false;

        string typeName = $"{scalar.Value[0].ToString().ToUpper()}{scalar.Value[1..]}Component";

        List<Type> types = Types
            .Where(type => type.Name == typeName)
            .ToList();

        Type? resolvedType = types.FirstOrDefault(type => !Attribute.IsDefined(type, typeof(ProtocolIdAttribute))) ?? types.FirstOrDefault();

        if (resolvedType != null)
            Type = resolvedType;

        return false;
    }

    [GeneratedRegex("^[a-zA-Z]+$")]
    private static partial Regex MyRegex();
}

public class Vector3TypeConverter : IYamlTypeConverter {
    public bool Accepts(Type type) => type == typeof(Vector3);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer) {
        parser.Consume<MappingStart>();
        Vector3 vector = default;

        while (parser.Current is Scalar property) {
            switch (property.Value.ToLowerInvariant()) {
                case "x":
                    parser.MoveNext();

                    vector.X = float.Parse(parser.Consume<Scalar>()
                        .Value);

                    break;

                case "y":
                    parser.MoveNext();

                    vector.Y = float.Parse(parser.Consume<Scalar>()
                        .Value);

                    break;

                case "z":
                    parser.MoveNext();

                    vector.Z = float.Parse(parser.Consume<Scalar>()
                        .Value);

                    break;
            }
        }

        parser.Consume<MappingEnd>();
        return vector;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer) {
        Vector3 vector = value as Vector3? ?? default;
        emitter.Emit(new MappingStart());

        emitter.Emit(new Scalar("x"));
        emitter.Emit(new Scalar(vector.X.ToString(CultureInfo.InvariantCulture)));

        emitter.Emit(new Scalar("y"));
        emitter.Emit(new Scalar(vector.Y.ToString(CultureInfo.InvariantCulture)));

        emitter.Emit(new Scalar("z"));
        emitter.Emit(new Scalar(vector.Z.ToString(CultureInfo.InvariantCulture)));

        emitter.Emit(new MappingEnd());
    }
}
