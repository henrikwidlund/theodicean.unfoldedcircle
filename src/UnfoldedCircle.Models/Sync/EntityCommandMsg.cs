namespace UnfoldedCircle.Models.Sync;

public sealed record MediaPlayerEntityCommandMsgData<TCommandId> : CommonReq<EntityCommandMsgData<TCommandId, MediaPlayerEntityCommandParams>>
    where TCommandId : struct, Enum;
