namespace UnfoldedCircle.Models.Sync;

public sealed record SelectEntityCommandMsgData : CommonReq<EntityCommandMsgData<SelectCommandId, SelectEntityCommandParams>>;
