using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.GZip;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Utils;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Vint.Core.Config;

public static class ConfigManager {
    public static IReadOnlyDictionary<string, MapInfo> MapInfos { get; private set; } = null!;

    public static List<string> GlobalEntitiesTypeNames => Root.Children
        .Where(child => child.Value.Entities.Count != 0)
        .Select(child => child.Key)
        .ToList();

    static ILogger Logger { get; } = Log.Logger.ForType(typeof(ConfigManager));
    static string ResourcesPath { get; } =
        Path.Combine(Directory.GetCurrentDirectory(), "Resources");

    static Dictionary<string, byte[]> LocaleToConfigCache { get; } = new(2);
    static ConfigNode Root { get; } = new();

    public static void InitializeMapInfos() {
        Logger.Information("Generating map infos");

        string mapInfosConfigPath = Path.Combine(ResourcesPath, "mapInfo.json");
        MapInfos = JsonConvert.DeserializeObject<Dictionary<string, MapInfo>>(File.ReadAllText(mapInfosConfigPath))!;

        Logger.Information("Map infos generated");
    }

    public static void InitializeCache() {
        Logger.Information("Generating config archives");

        string configsPath = Path.Combine(ResourcesPath, "StaticServer", "config");

        foreach (string configDir in Directory.EnumerateDirectories(configsPath)) {
            string locale = new DirectoryInfo(configDir).Name;

            Logger.Debug("Generating archive for the '{Locale}' locale", locale);

            using MemoryStream outStream = new();

            using (IWriter writer = WriterFactory.Open(outStream, ArchiveType.Tar, new GZipWriterOptions())) {
                writer.WriteAll(configDir, "*", SearchOption.AllDirectories);
            }

            byte[] buffer = outStream.ToArray();

            LocaleToConfigCache[locale] = buffer;
        }

        Logger.Information("Cache for config archives generated");
    }

    public static void InitializeNodes() {
        Logger.Information("Generating config nodes");

        string configsPath = Path.Combine(ResourcesPath, "StaticServer", "config");

        IDeserializer deserializer = new DeserializerBuilder()
            .WithNodeDeserializer(new ComponentDeserializer())
            .WithNodeTypeResolver(new ComponentDeserializer())
            .IgnoreUnmatchedProperties()
            .Build();

        Dictionary<string, object?> components = new();
        Dictionary<string, long?> ids = new();

        string rootPath = Path.Combine(configsPath, "ru");

        foreach (string filePath in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)) {
            string relativePath = Path.GetRelativePath(rootPath, filePath).Replace('\\', '/');
            string fileName = Path.GetFileName(filePath);

            if (string.IsNullOrEmpty(fileName)) continue;

            Logger.Verbose("Parsing {File}", relativePath);

            switch (fileName) {
                case "id.yml": {
                    Dictionary<string, long> obj =
                        new Deserializer().Deserialize<Dictionary<string, long>>(File.ReadAllText(filePath));

                    ids[relativePath[..^7]] = obj["id"];
                    break;
                }

                case "public.yml": {
                    components[relativePath[..^11]] = deserializer.Deserialize(File.ReadAllText(filePath));
                    break;
                }
            }

            foreach ((string key, object? value) in components) {
                if (value is not Dictionary<object, object> dict ||
                    dict.Values.All(v => v is not IComponent))
                    continue;

                ConfigNode curNode = Root;

                foreach (string part in key.Split('/')) {
                    if (curNode.Children.TryGetValue(part, out ConfigNode? child))
                        curNode = child;
                    else {
                        ids.TryGetValue(key, out long? id);
                        curNode = curNode.Children[part] = new ConfigNode { Id = id };
                    }
                }

                foreach (object obj in dict.Values) {
                    if (obj is not IComponent component) continue;

                    Type componentType = component.GetType();

                    if (componentType.IsDefined(typeof(ProtocolIdAttribute)))
                        curNode.Components[componentType] = component;
                    else
                        curNode.ServerComponents[componentType] = component;
                }

                foreach (IComponent serverComponent in curNode.ServerComponents.Values) {
                    foreach (Type type in serverComponent.GetType()
                                 .GetInterfaces()
                                 .Where(i => i.IsGenericType &&
                                             i.GetGenericTypeDefinition() == typeof(IConvertible<>))) {
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

    public static void InitializeGlobalEntities() {
        Logger.Information("Generating global entities");

        List<Type> types = Assembly.GetExecutingAssembly().GetTypes().ToList();
        string rootPath = Path.Combine(ResourcesPath, "GlobalEntities");

        Dictionary<string, Dictionary<string, IEntity>> globalEntities = new();

        List<string> typesToLoad = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Path.Combine(rootPath, "typesToLoad.json")))!;

        foreach (string filePath in typesToLoad.Select(type => Path.Combine(rootPath, $"{type}.json"))) {
            string relativePath = Path.GetRelativePath(rootPath, filePath).Replace('\\', '/');
            string entitiesTypeName = Path.GetFileNameWithoutExtension(filePath);

            Logger.Verbose("Parsing {File}", relativePath);

            JArray jArray = JArray.Parse(File.ReadAllText(filePath));

            Dictionary<string, IEntity> entities = new(jArray.Count);

            foreach (JToken jToken in jArray) {
                string entityName = jToken["name"]!.ToObject<string>()!;

                Logger.Verbose("Generating '{Name}'", entityName);

                long entityId = jToken["id"]!.ToObject<long>();

                if (entityId == 0)
                    entityId = EntityRegistry.FreeId;

                JArray templateComponents = jToken["template"]!.ToObject<JArray>()!;
                string templateName = templateComponents[0].ToObject<string>()!;
                string configPath = templateComponents[1].ToObject<string>()!;

                JObject rawComponents = jToken["components"]!.ToObject<JObject>()!;

                List<IComponent> components = new(rawComponents.Count);

                foreach ((string rawComponentName, JToken? rawComponentProperties) in rawComponents) {
                    Type componentType = types.Single(type => type.Name == rawComponentName);
                    ConstructorInfo componentCtor = componentType.GetConstructors().First();
                    ParameterInfo[] ctorParameters = componentCtor.GetParameters();

                    List<object?> parameters = new(ctorParameters.Length);

                    parameters.AddRange(ctorParameters
                        .Select(ctorParameter => {
                            JToken? rawComponentProperty = rawComponentProperties![ctorParameter.Name!];

                            if (rawComponentProperty == null && ctorParameter.HasDefaultValue)
                                return ctorParameter.DefaultValue;

                            return rawComponentProperty?.ToObject(ctorParameter.ParameterType);
                        }));

                    components.Add((IComponent)componentCtor.Invoke(parameters.ToArray()));
                }

                Type templateType = types.Single(type => type.Name == templateName);
                ConstructorInfo templateCtor = templateType.GetConstructors().First();
                EntityTemplate template = (EntityTemplate)templateCtor.Invoke(null);

                IEntityBuilder entityBuilder = new EntityBuilder(entityId).WithTemplateAccessor(template, configPath);
                components.ForEach(component => entityBuilder.AddComponent(component));

                IEntity entity = entityBuilder.Build();

                entities[entityName] = entity;

                Logger.Verbose("Generated {Entity}", entity);
            }

            globalEntities[entitiesTypeName] = entities;
        }

        Logger.Debug("Generating nodes for global entities");

        foreach ((string entitiesTypeName, Dictionary<string, IEntity> entities) in globalEntities) {
            ConfigNode curNode = Root;

            if (curNode.Children.TryGetValue(entitiesTypeName, out ConfigNode? child))
                curNode = child;
            else
                curNode = curNode.Children[entitiesTypeName] = new ConfigNode();

            foreach ((string entityName, IEntity entity) in entities)
                curNode.Entities[entityName] = entity;
        }

        Logger.Information("Global entities generated");
    }

    public static IEntity GetGlobalEntity(string path, string entityName) {
        ConfigNode node = GetNode(path)!;

        return node.Entities[entityName].Clone();
    }

    public static IEnumerable<IEntity> GetGlobalEntities(string path) {
        ConfigNode node = GetNode(path)!;

        return node.Entities.Values.Select(entity => entity.Clone());
    }

    public static IEnumerable<IEntity> GetGlobalEntities() {
        string[] excluded = ["moduleSlots"];

        return Root.Children
            .Where(child => !excluded.Contains(child.Key))
            .SelectMany(child =>
                child.Value.Entities.Values.Select(entity => entity.Clone()));
    }

    public static T GetComponent<T>(string path) where T : class, IComponent =>
        GetComponentOrNull<T>(path)!;

    public static T? GetComponentOrNull<T>(string path) where T : class, IComponent {
        ConfigNode? node = GetNode(path);

        if (node == null) return null;

        if (!node.Components.TryGetValue(typeof(T), out IComponent? component))
            node.ServerComponents.TryGetValue(typeof(T), out component);

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
            if (curNode.Children.TryGetValue(part, out ConfigNode? child))
                curNode = child;
            else return null;

        return curNode;
    }

    class ConfigNode {
        public long? Id { get; init; }
        public Dictionary<Type, IComponent> Components { get; } = new();
        public Dictionary<Type, IComponent> ServerComponents { get; } = new();
        public Dictionary<string, IEntity> Entities { get; } = new();
        public Dictionary<string, ConfigNode> Children { get; } = new();
    }
}

// Copied from https://github.com/Assasans/TXServer-Public/blob/database/TXServer/Core/Configuration/ComponentDeserializer.cs
public partial class ComponentDeserializer : INodeTypeResolver, INodeDeserializer {
    ILogger Logger { get; } = Log.Logger.ForType(typeof(ComponentDeserializer));
    Type? Type { get; set; }

    IEnumerable<Type> Types { get; } = Assembly.GetExecutingAssembly().GetTypes().ToList();

    public bool Deserialize(
        IParser reader,
        Type expectedType,
        Func<IParser, Type, object?> nestedObjectDeserializer,
        out object? retValue) {
        if (!typeof(IComponent).IsAssignableFrom(expectedType)) {
            retValue = null;
            return false;
        }

        IComponent component = (IComponent)RuntimeHelpers.GetUninitializedObject(expectedType);

        reader.MoveNext();

        while (reader.Current != null && reader.Current is not MappingEnd) {
            if (reader.Current is not Scalar scalar) continue;

            string key = scalar.Value[0].ToString().ToUpper() + scalar.Value[1..];
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
            !MyRegex().IsMatch(scalar.Value)) return false;

        string typeName =
            $"{scalar.Value[0].ToString().ToUpper()}{scalar.Value[1..]}Component";

        List<Type> types = Types.Where(type => type.Name == typeName).ToList();

        Type? resolvedType = types.FirstOrDefault(type => !Attribute.IsDefined(type, typeof(ProtocolIdAttribute))) ??
                             types.FirstOrDefault();

        if (resolvedType != null)
            Type = resolvedType;

        return false;
    }

    [GeneratedRegex("^[a-zA-Z]+$")]
    private static partial Regex MyRegex();
}