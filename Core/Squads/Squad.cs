using System.Collections.Concurrent;
using Vint.Core.Chat.Components;
using Vint.Core.Config;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Squads.Components;
using Vint.Core.Squads.Templates;

namespace Vint.Core.Squads;

public class Squad : IDisposable {
    static SquadConfigComponent Config { get; } = ConfigManager.GetComponent<SquadConfigComponent>("/squad");

    readonly ConcurrentDictionary<long, IPlayerConnection> _members = [];

    public IPlayerConnection Leader { get; private set; } = null!;
    public IEntity Entity { get; } = new SquadTemplate().Create();
    public IEntity ChatEntity { get; } = new SquadChatTemplate().Create();

    public ICollection<IPlayerConnection> Members => _members.Values;
    public bool HasSpace => _members.Count < Config.MaxSquadSize;
    public bool Disbanded { get; private set; }

    public bool CanAddMember => HasSpace && !Disbanded;

    public async Task SetLeader(long memberId) {
        if (!_members.TryGetValue(memberId, out IPlayerConnection? member))
            throw new InvalidOperationException("Player is not in the squad");

        if (Leader != null!)
            await Leader.UserContainer.Entity.RemoveComponent<SquadLeaderComponent>();

        Leader = member;
        await member.UserContainer.Entity.AddComponent<SquadLeaderComponent>();
    }

    public async Task Disband() {
        Disbanded = true;

        foreach (long memberId in _members.Keys) {
            if (!_members.TryRemove(memberId, out IPlayerConnection? member))
                continue;

            await HandleMemberRemove(member);
        }

        Dispose();
    }

    public async Task AddMember(IPlayerConnection member) {
        if (!CanAddMember || member.InSquad || !_members.TryAdd(member.UserContainer.Id, member))
            return;

        await HandleMemberAdd(member);
    }

    public async Task RemoveMember(long memberId) {
        if (!_members.TryRemove(memberId, out IPlayerConnection? member))
            return;

        await HandleMemberRemove(member);

        if (_members.Count == 1 || member == Leader)
            await Disband();
    }

    async Task HandleMemberAdd(IPlayerConnection member) {
        member.Squad = this;

        await ChatEntity.ChangeComponent<ChatParticipantsComponent>(component => component.Users.Add(member.UserContainer.Entity));

        await member.Share(Entity, ChatEntity);
        await member.UserContainer.Entity.AddGroupComponent<SquadGroupComponent>(Entity);

        foreach (IPlayerConnection otherMember in Members.Where(m => m != member)) {
            await member.UserContainer.ShareTo(otherMember);
            await otherMember.UserContainer.ShareTo(member);
        }
    }

    async Task HandleMemberRemove(IPlayerConnection member) {
        member.Squad = null;

        await member.UserContainer.Entity.RemoveComponentIfPresent<SquadLeaderComponent>();

        foreach (IPlayerConnection otherMember in Members.Where(m => m != member)) {
            await otherMember.UserContainer.UnshareFrom(member);
            await member.UserContainer.UnshareFrom(otherMember);
        }

        await member.UserContainer.Entity.RemoveComponent<SquadGroupComponent>();
        await member.Unshare(ChatEntity, Entity);

        await ChatEntity.ChangeComponent<ChatParticipantsComponent>(component => component.Users.Remove(member.UserContainer.Entity));
    }

    public void Dispose() {
        ChatEntity.Dispose();
        Entity.Dispose();
        GC.SuppressFinalize(this);
    }
}
