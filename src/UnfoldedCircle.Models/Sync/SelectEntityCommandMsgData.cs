namespace UnfoldedCircle.Models.Sync;

public record SelectEntityCommandMsgData : CommonReq<EntityCommandMsgData<SelectCommandId, SelectEntityCommandParams>>;