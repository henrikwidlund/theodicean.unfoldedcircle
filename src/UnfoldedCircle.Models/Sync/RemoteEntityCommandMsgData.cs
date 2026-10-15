namespace UnfoldedCircle.Models.Sync;

public sealed record RemoteEntityCommandMsgData : CommonReq<EntityCommandMsgData<string, RemoteEntityCommandParams>>;
