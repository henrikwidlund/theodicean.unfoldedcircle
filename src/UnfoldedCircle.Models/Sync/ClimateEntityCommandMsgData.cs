namespace UnfoldedCircle.Models.Sync;

public sealed record ClimateEntityCommandMsgData : CommonReq<EntityCommandMsgData<ClimateCommandId, ClimateEntityCommandParams>>;
