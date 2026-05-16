namespace MLN111.Dto.Ai;

public sealed record ChatResponse(string Reply, string Model);

public sealed record AiStatusResponse(bool Enabled, bool Configured, string Model, string Provider = "Grok");
