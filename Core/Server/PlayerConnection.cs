using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using LinqToDB;
using Serilog;
using Vint.Core.Battles.Player;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Battle.User;
using Vint.Core.ECS.Components.Entrance;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Components.Preset;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.ECS.Events.Entrance.Login;
using Vint.Core.ECS.Events.Items;
using Vint.Core.ECS.Templates.Avatar;
using Vint.Core.ECS.Templates.Covers;
using Vint.Core.ECS.Templates.Entrance;
using Vint.Core.ECS.Templates.Gold;
using Vint.Core.ECS.Templates.Graffiti;
using Vint.Core.ECS.Templates.Hulls;
using Vint.Core.ECS.Templates.Modules;
using Vint.Core.ECS.Templates.Money;
using Vint.Core.ECS.Templates.Paints;
using Vint.Core.ECS.Templates.Shells;
using Vint.Core.ECS.Templates.Skins;
using Vint.Core.ECS.Templates.User;
using Vint.Core.ECS.Templates.Weapons.Market;
using Vint.Core.ECS.Templates.Weapons.User;
using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Protocol.Commands;
using Vint.Core.Utils;

namespace Vint.Core.Server;

public interface IPlayerConnection {
    public ILogger Logger { get; }

    public GameServer Server { get; }
    public Player Player { get; set; }
    public BattlePlayer? BattlePlayer { get; set; }
    public IEntity User { get; }
    public IEntity ClientSession { get; }

    public bool IsOnline { get; }
    public bool InLobby { get; }
    public Invite? Invite { get; set; }

    public HashSet<IEntity> SharedEntities { get; }
    public Dictionary<string, HashSet<IEntity>> UserEntities { get; }

    public void Register(
        string username,
        string encryptedPasswordDigest,
        string email,
        string hardwareFingerprint,
        bool subscribed,
        bool steam,
        bool quickRegistration);

    public void Login(
        bool saveAutoLoginToken,
        string hardwareFingerprint);

    public void ChangePassword(string passwordDigest);

    public void ChangeReputation(int reputation);

    public void PurchaseItem(IEntity marketItem, int amount, int price, bool forXCrystals, bool mount);

    public void MountItem(IEntity userItem);

    public void SetUsername(string username);

    public void SetCrystals(long crystals);

    public void SetXCrystals(long xCrystals);

    public void SetGoldBoxes(int goldBoxes);

    public void Kick(string? reason);

    public void Send(ICommand command);

    public void Send(IEvent @event);

    public void Send(IEvent @event, params IEntity[] entities);

    public void Share(IEntity entity);

    public void ShareIfUnshared(IEntity entity);

    public void Unshare(IEntity entity);

    public void UnshareIfShared(IEntity entity);
}

public abstract class PlayerConnection(
    GameServer server
) : IPlayerConnection {
    public Guid Id { get; } = Guid.NewGuid();
    public ILogger Logger { get; protected set; } = Log.Logger.ForType(typeof(PlayerConnection));
    public Dictionary<string, HashSet<IEntity>> UserEntities { get; } = new();

    public GameServer Server { get; } = server;
    public Player Player { get; set; } = null!;
    public IEntity User { get; private set; } = null!;
    public IEntity ClientSession { get; protected set; } = null!;
    public BattlePlayer? BattlePlayer { get; set; }
    public HashSet<IEntity> SharedEntities { get; private set; } = [];

    public abstract bool IsOnline { get; }
    public bool InLobby => BattlePlayer != null;
    public Invite? Invite { get; set; }

    public void Register(
        string username,
        string encryptedPasswordDigest,
        string email,
        string hardwareFingerprint,
        bool subscribed,
        bool steam,
        bool quickRegistration) {
        Logger.Information("Registering player '{Username}'", username);

        byte[] passwordHash = new Encryption().RsaDecrypt(Convert.FromBase64String(encryptedPasswordDigest));

        Player = new Player {
            Id = EntityRegistry.FreeId,
            Username = username,
            Email = email,
            CountryCode = ClientSession.GetComponent<ClientLocaleComponent>().LocaleCode,
            HardwareFingerprint = hardwareFingerprint,
            Subscribed = subscribed,
            RegistrationTime = DateTimeOffset.UtcNow,
            LastLoginTime = DateTimeOffset.UtcNow,
            PasswordHash = passwordHash
        };

        using (DbConnection db = new()) {
            db.Insert(Player);

            if (Invite != null) {
                Invite.RemainingUses--;
                db.Update(Invite);
            }
        }

        Player.InitializeNew();

        Login(true, hardwareFingerprint);
    }

    public void Login(
        bool saveAutoLoginToken,
        string hardwareFingerprint) {
        Logger = Logger.WithPlayer((SocketPlayerConnection)this);

        Player.RememberMe = saveAutoLoginToken;
        Player.LastLoginTime = DateTimeOffset.UtcNow;
        Player.HardwareFingerprint = hardwareFingerprint;

        if (saveAutoLoginToken) {
            Encryption encryption = new();

            byte[] autoLoginToken = new byte[32];
            new Random().NextBytes(autoLoginToken);

            byte[] encryptedAutoLoginToken = encryption.EncryptAutoLoginToken(autoLoginToken, Player.PasswordHash);

            Player.AutoLoginToken = autoLoginToken;
            Send(new SaveAutoLoginTokenEvent(Player.Username, encryptedAutoLoginToken));
        }

        User = new UserTemplate().Create(Player);
        Share(User);

        ClientSession.AddComponent(User.GetComponent<UserGroupComponent>());

        Logger.Warning("Logged in");

        using DbConnection db = new();
        db.Update(Player);
    }

    public void ChangePassword(string passwordDigest) {
        Encryption encryption = new();

        byte[] passwordHash = encryption.RsaDecrypt(Convert.FromBase64String(passwordDigest));
        Player.PasswordHash = passwordHash;

        using DbConnection database = new();

        database.Players
            .Where(player => player.Id == Player.Id)
            .Set(player => player.PasswordHash, Player.PasswordHash)
            .Update();
    }

    public void ChangeReputation(int reputation) {
        using DbConnection db = new();
        DateOnly date = DateOnly.FromDateTime(DateTime.Today);

        SeasonStatistics seasonStats = db.SeasonStatistics
            .Where(stats => stats.PlayerId == Player.Id)
            .OrderByDescending(stats => stats.SeasonNumber)
            .First();

        ReputationStatistics? reputationStats = db.ReputationStatistics
            .SingleOrDefault(repStats => repStats.PlayerId == Player.Id &&
                                         repStats.Date == date);

        int oldLeagueIndex = seasonStats.LeagueIndex;

        reputationStats ??= new ReputationStatistics {
            Player = Player,
            Date = date,
            SeasonNumber = seasonStats.SeasonNumber
        };

        seasonStats.Reputation = reputation;
        reputationStats.Reputation = reputation;

        User.ChangeComponent<UserReputationComponent>(component => component.Reputation = reputation);

        if (oldLeagueIndex != seasonStats.LeagueIndex) {
            User.RemoveComponent<LeagueGroupComponent>();
            User.AddComponent(seasonStats.League.GetComponent<LeagueGroupComponent>());
        }

        db.Update(seasonStats);
        db.InsertOrReplace(reputationStats);
    }

    public void PurchaseItem(IEntity marketItem, int amount, int price, bool forXCrystals, bool mount) {
        using DbConnection db = new();
        IEntity? userItem = null;

        switch (marketItem.TemplateAccessor!.Template) {
            case AvatarMarketItemTemplate: {
                db.Insert(new Avatar { Player = Player, Id = marketItem.Id });
                break;
            }

            case GraffitiMarketItemTemplate or ChildGraffitiMarketItemTemplate: {
                db.Insert(new Graffiti { Player = Player, Id = marketItem.Id });
                break;
            }

            case CrystalMarketItemTemplate: {
                SetCrystals(Player.Crystals + amount);
                db.Update(Player);
                break;
            }

            case XCrystalMarketItemTemplate: {
                SetXCrystals(Player.XCrystals + amount);
                db.Update(Player);
                break;
            }

            case GoldBonusMarketItemTemplate: {
                SetGoldBoxes(Player.GoldBoxItems + amount);
                db.Update(Player);
                break;
            }

            case TankMarketItemTemplate: {
                long skinId = GlobalEntities.DefaultSkins[marketItem.Id];

                db.Insert(new Hull { Player = Player, Id = marketItem.Id, SkinId = skinId });
                PurchaseItem(GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == skinId), 1, 0, false, mount);
                break;
            }

            case WeaponMarketItemTemplate: {
                long skinId = GlobalEntities.DefaultSkins[marketItem.Id];
                long shellId = GlobalEntities.DefaultShells[marketItem.Id];

                db.Insert(new Weapon { Player = Player, Id = marketItem.Id, SkinId = skinId, ShellId = shellId });
                PurchaseItem(GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == skinId), 1, 0, false, mount);
                PurchaseItem(GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == shellId), 1, 0, false, mount);
                break;
            }

            case HullSkinMarketItemTemplate: {
                long hullId = marketItem.GetComponent<ParentGroupComponent>().Key;

                db.Insert(new HullSkin { Player = Player, Id = marketItem.Id, HullId = hullId });
                break;
            }

            case WeaponSkinMarketItemTemplate: {
                long weaponId = marketItem.GetComponent<ParentGroupComponent>().Key;

                db.Insert(new WeaponSkin { Player = Player, Id = marketItem.Id, WeaponId = weaponId });
                break;
            }

            case TankPaintMarketItemTemplate: {
                db.Insert(new Paint { Player = Player, Id = marketItem.Id });
                break;
            }

            case WeaponPaintMarketItemTemplate: {
                db.Insert(new Cover { Player = Player, Id = marketItem.Id });
                break;
            }

            case ShellMarketItemTemplate: {
                long weaponId = marketItem.GetComponent<ParentGroupComponent>().Key;

                db.Insert(new Shell { Player = Player, Id = marketItem.Id, WeaponId = weaponId });
                break;
            }

            case ModuleCardMarketItemTemplate: {
                long moduleId = marketItem.GetComponent<ParentGroupComponent>().Key;
                Module? module = db.Modules
                    .Where(module => module.PlayerId == Player.Id)
                    .SingleOrDefault(module => module.Id == moduleId);

                module ??= new Module { Player = Player, Id = moduleId };
                module.Cards += amount;

                db.Insert(module);
                break;
            }

            default: throw new NotImplementedException();
        }

        userItem ??= marketItem.GetUserEntity(this);
        userItem.AddComponentIfAbsent(new UserGroupComponent(User));

        if (price > 0) {
            if (forXCrystals) SetXCrystals(Player.XCrystals - price);
            else SetCrystals(Player.Crystals - price);

            db.Update(Player);
        }

        if (userItem.HasComponent<UserItemCounterComponent>() &&
            userItem.TemplateAccessor!.Template is GoldBonusMarketItemTemplate) {
            userItem.ChangeComponent<UserItemCounterComponent>(component => component.Count += amount);
            Send(new ItemsCountChangedEvent(amount), userItem);
        }

        if (mount) MountItem(userItem);
    }

    public void MountItem(IEntity userItem) {
        using DbConnection db = new();
        Preset currentPreset = Player.CurrentPreset;
        IEntity marketItem = userItem.GetMarketEntity(this);

        switch (userItem.TemplateAccessor!.Template) {
            case AvatarUserItemTemplate: {
                this.GetEntity(Player.CurrentAvatarId)!.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                userItem.AddComponent(new MountedItemComponent());

                Player.CurrentAvatarId = marketItem.Id;
                User.ChangeComponent(new UserAvatarComponent(this, Player.CurrentAvatarId));

                db.Update(Player);
                break;
            }

            case GraffitiUserItemTemplate: {
                currentPreset.Graffiti.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                currentPreset.Graffiti = marketItem;
                userItem.AddComponent(new MountedItemComponent());

                db.Update(currentPreset);
                break;
            }

            case TankUserItemTemplate: {
                currentPreset.Hull.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                currentPreset.Hull = marketItem;
                userItem.AddComponent(new MountedItemComponent());
                currentPreset.Entity!.GetComponent<PresetEquipmentComponent>().SetHullId(currentPreset.Hull.Id);

                Hull newHull = db.Hulls
                    .Where(hull => hull.PlayerId == Player.Id)
                    .Single(hull => hull.Id == currentPreset.Hull.Id);

                IEntity skin = GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == newHull.SkinId);

                currentPreset.HullSkin.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                currentPreset.HullSkin = skin;
                currentPreset.HullSkin.GetUserEntity(this).AddComponent(new MountedItemComponent());

                db.Update(currentPreset);
                break;
            }

            case WeaponUserItemTemplate: {
                currentPreset.Weapon.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                currentPreset.Weapon = marketItem;
                userItem.AddComponent(new MountedItemComponent());
                currentPreset.Entity!.GetComponent<PresetEquipmentComponent>().SetWeaponId(currentPreset.Weapon.Id);

                Weapon newWeapon = db.Weapons
                    .Where(weapon => weapon.PlayerId == Player.Id)
                    .Single(weapon => weapon.Id == currentPreset.Weapon.Id);

                IEntity skin = GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == newWeapon.SkinId);
                IEntity shell = GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == newWeapon.ShellId);

                currentPreset.WeaponSkin.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                currentPreset.WeaponSkin = skin;
                currentPreset.WeaponSkin.GetUserEntity(this).AddComponent(new MountedItemComponent());

                currentPreset.Shell.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                currentPreset.Shell = shell;
                currentPreset.Shell.GetUserEntity(this).AddComponent(new MountedItemComponent());

                db.Update(currentPreset);
                break;
            }

            case HullSkinUserItemTemplate: {
                HullSkin skin = db.HullSkins
                    .Where(skin => skin.PlayerId == Player.Id)
                    .Single(skin => skin.Id == marketItem.Id);

                if (skin.HullId != currentPreset.Hull.Id) return;

                currentPreset.HullSkin.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                currentPreset.HullSkin = marketItem;
                userItem.AddComponent(new MountedItemComponent());

                db.Hulls
                    .Where(hull => hull.PlayerId == Player.Id &&
                                   hull.Id == currentPreset.Hull.Id)
                    .Set(hull => hull.SkinId, currentPreset.HullSkin.Id)
                    .Update();

                db.Update(currentPreset);
                break;
            }

            case WeaponSkinUserItemTemplate: {
                WeaponSkin skin = db.WeaponSkins
                    .Where(skin => skin.PlayerId == Player.Id)
                    .Single(skin => skin.Id == marketItem.Id);

                if (skin.WeaponId != currentPreset.Weapon.Id) return;

                currentPreset.WeaponSkin.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                currentPreset.WeaponSkin = marketItem;
                userItem.AddComponent(new MountedItemComponent());

                db.Weapons
                    .Where(weapon => weapon.PlayerId == Player.Id &&
                                     weapon.Id == currentPreset.Weapon.Id)
                    .Set(weapon => weapon.SkinId, currentPreset.WeaponSkin.Id)
                    .Update();

                db.Update(currentPreset);
                break;
            }

            case TankPaintUserItemTemplate: {
                currentPreset.Paint.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                currentPreset.Paint = marketItem;
                userItem.AddComponent(new MountedItemComponent());

                db.Update(currentPreset);
                break;
            }

            case WeaponPaintUserItemTemplate: {
                currentPreset.Cover.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                currentPreset.Cover = marketItem;
                userItem.AddComponent(new MountedItemComponent());

                db.Update(currentPreset);
                break;
            }

            case ShellUserItemTemplate: {
                Shell shell = db.Shells
                    .Where(shell => shell.PlayerId == Player.Id)
                    .Single(shell => shell.Id == marketItem.Id);

                if (shell.WeaponId != currentPreset.Weapon.Id) return;

                currentPreset.Shell.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                currentPreset.Shell = marketItem;
                userItem.AddComponent(new MountedItemComponent());

                db.Weapons
                    .Where(weapon => weapon.PlayerId == Player.Id &&
                                     weapon.Id == currentPreset.Weapon.Id)
                    .Set(weapon => weapon.ShellId, currentPreset.Shell.Id)
                    .Update();

                db.Update(currentPreset);
                break;
            }

            default: throw new NotImplementedException();
        }

        if (!User.HasComponent<UserEquipmentComponent>()) return;

        User.RemoveComponent<UserEquipmentComponent>();
        User.AddComponent(new UserEquipmentComponent(Player.CurrentPreset.Weapon.Id, Player.CurrentPreset.Hull.Id));
    }

    public void SetUsername(string username) {
        Player.Username = username;
        User.ChangeComponent<UserUidComponent>(component => component.Username = username);
    }

    public void SetCrystals(long crystals) {
        Player.Crystals = crystals;
        User.ChangeComponent<UserMoneyComponent>(component => component.Money = Player.Crystals);
    }

    public void SetXCrystals(long xCrystals) {
        Player.XCrystals = xCrystals;
        User.ChangeComponent<UserXCrystalsComponent>(component => component.Money = Player.XCrystals);
    }

    public void SetGoldBoxes(int goldBoxes) {
        Player.GoldBoxItems = goldBoxes;
        SharedEntities.Single(entity => entity.TemplateAccessor!.Template is GoldBonusUserItemTemplate)
            .ChangeComponent<UserItemCounterComponent>(component =>
                component.Count = Player.GoldBoxItems);
    }

    public abstract void Kick(string? reason);

    public abstract void Send(ICommand command);

    public void Send(IEvent @event) => ClientSession.Send(@event);

    public void Send(IEvent @event, params IEntity[] entities) => Send(new SendEventCommand(@event, entities));

    public void Share(IEntity entity) => entity.Share(this);

    public void ShareIfUnshared(IEntity entity) {
        if (!SharedEntities.Contains(entity))
            Share(entity);
    }

    public void Unshare(IEntity entity) => entity.Unshare(this);

    public void UnshareIfShared(IEntity entity) {
        if (SharedEntities.Contains(entity))
            Unshare(entity);
    }

    public override int GetHashCode() => Id.GetHashCode();

    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
    public override string ToString() => $"PlayerConnection {{ " +
                                         $"ClientSession Id: '{ClientSession?.Id}'; " +
                                         $"Username: '{Player?.Username}' }}";
}

public class SocketPlayerConnection(
    GameServer server,
    Socket socket,
    Protocol.Protocol protocol
) : PlayerConnection(server) {
    public IPEndPoint EndPoint { get; } = (IPEndPoint)socket.RemoteEndPoint!;

    public override bool IsOnline => IsConnected && IsSocketConnected && ClientSession != null! && User != null! && Player != null!;
    bool IsSocketConnected => Socket.Connected;
    bool IsConnected { get; set; }

    Socket Socket { get; } = socket;
    Protocol.Protocol Protocol { get; } = protocol;
    BlockingCollection<ICommand> ExecuteBuffer { get; } = new();
    BlockingCollection<ICommand> SendBuffer { get; } = new();

    public override void Kick(string? reason) {
        Logger.Warning("Player kicked (reason: '{Reason}')", reason);
        Disconnect();
    }

    public void OnConnected() {
        Logger = Logger.WithEndPoint(EndPoint);

        ClientSession = new ClientSessionTemplate().Create();
        Logger.Information("New socket connected");

        Send(new InitTimeCommand(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        Share(ClientSession);

        Task.Run(ReceiveLoop).Catch();
        Task.Run(SendLoop).Catch();
        Task.Run(ExecuteLoop).Catch();

        IsConnected = true;
    }

    public override void Send(ICommand command) {
        if (!IsSocketConnected || SendBuffer.IsAddingCompleted) return;

        Logger.Debug("Queueing for sending {Command}", command);
        SendBuffer.Add(command);
    }

    public void Disconnect() {
        if (!IsConnected) return;
        
        try {
            Socket.Shutdown(SocketShutdown.Both);
        } finally {
            Socket.Close();
            OnDisconnected();
        }
    }

    void OnDisconnected() {
        if (!IsConnected) return;

        IsConnected = false;
        
        Logger.Information("Socket disconnected");
        Logger.Warning("{X}", new StackTrace());

        try {
            if (InLobby) {
                if (BattlePlayer!.InBattleAsTank || BattlePlayer.IsSpectator)
                    BattlePlayer.Battle.RemovePlayer(BattlePlayer);
                else
                    BattlePlayer.Battle.RemovePlayerFromLobby(BattlePlayer);
            }

            if (User != null!)
                EntityRegistry.Remove(User.Id);
        } catch (Exception e) {
            Logger.Error(e, "Caught an exception while disconnecting socket");
        } finally {
            SendBuffer.CompleteAdding();
            ExecuteBuffer.CompleteAdding();

            foreach (IEntity entity in SharedEntities)
                entity.SharedPlayers.Remove(this);

            SharedEntities.Clear();
            UserEntities.Clear();
            SendBuffer.Dispose();
            ExecuteBuffer.Dispose();
        }
    }

    async Task ReceiveLoop() {
        byte[] bytes = ArrayPool<byte>.Shared.Rent(4096);

        try {
            while (IsSocketConnected) {
                ProtocolBuffer buffer = new(new OptionalMap(), this);
                await using NetworkStream stream = new(Socket);
                using BinaryReader reader = new BigEndianBinaryReader(stream);

                if (!buffer.Unwrap(reader))
                    throw new InvalidDataException("Failed to unwrap packet");

                long availableForRead = buffer.Stream.Length - buffer.Stream.Position;

                while (availableForRead > 0) {
                    Logger.Verbose("Decode buffer bytes available: {Count}", availableForRead);

                    ICommand command = (ICommand)Protocol.GetCodec(new TypeCodecInfo(typeof(ICommand))).Decode(buffer);

                    Logger.Debug("Queueing for executing: {Command}", command);
                    ExecuteBuffer.Add(command);

                    availableForRead = buffer.Stream.Length - buffer.Stream.Position;
                }

                Array.Clear(bytes);
            }
        } catch (IOException ioEx) {
            if (ioEx.InnerException is SocketException sEx) {
                switch (sEx.SocketErrorCode) {
                    case SocketError.Shutdown:
                    case SocketError.OperationAborted:
                    case SocketError.ConnectionReset:
                    case SocketError.ConnectionRefused:
                    case SocketError.ConnectionAborted: {
                        Socket.Close();
                        OnDisconnected();
                        break;
                    }

                    default:
                        Logger.Error(sEx, "Socket caught an exception while receiving data");
                        Disconnect();
                        break;
                }
            } else {
                Logger.Error(ioEx, "Socket caught an exception while receiving data");
                Disconnect();
            }
        } catch (SocketException sEx) { // wtf??? sex??????????
            switch (sEx.SocketErrorCode) {
                case SocketError.Shutdown:
                case SocketError.OperationAborted:
                case SocketError.ConnectionReset:
                case SocketError.ConnectionRefused:
                case SocketError.ConnectionAborted: {
                    Socket.Close();
                    OnDisconnected();
                    break;
                }

                default:
                    Logger.Error(sEx, "Socket caught an exception while receiving data");
                    Disconnect();
                    break;
            }
        } catch (Exception ex) {
            Logger.Error(ex, "Socket caught an exception while receiving data");
            Disconnect();
        } finally {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }

    async Task SendLoop() {
        try {
            while (IsSocketConnected && !SendBuffer.IsCompleted) {
                ICommand command = SendBuffer.Take();
                
                if (!IsSocketConnected) return;

                try {
                    ProtocolBuffer buffer = new(new OptionalMap(), this);
                    Protocol.GetCodec(new TypeCodecInfo(typeof(ICommand))).Encode(buffer, command);

                    using MemoryStream stream = new();
                    await using BinaryWriter writer = new BigEndianBinaryWriter(stream);
                    buffer.Wrap(writer);

                    byte[] bytes = stream.ToArray();
                    await Socket.SendAsync(bytes);

                    Logger.Verbose("Sent {Command}: {Size} bytes ({Hex})", command, bytes.Length, Convert.ToHexString(bytes));
                } catch (Exception e) {
                    Logger.Error(e, "Failed to send {Command}", command);
                }
            }
        } catch (InvalidOperationException) { }
    }

    void ExecuteLoop() {
        try {
            while (!ExecuteBuffer.IsCompleted) {
                ICommand command = ExecuteBuffer.Take();

                try {
                    command.Execute(this);
                } catch (Exception e) {
                    Logger.Error(e, "Failed to execute {Command}", command);
                }
            }
        } catch (InvalidOperationException) { }
    }
}