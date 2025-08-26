namespace DocHub.Application.DTOs;

public class GenerateSignatureRequest
{
    public string AuthorityName { get; set; } = string.Empty;
    public string AuthorityDesignation { get; set; } = string.Empty;
}

public class PROXKeyStatus
{
    public bool IsConnected { get; set; }
    public bool IsValid { get; set; }
    public PROXKeyInfoDto? DeviceInfo { get; set; }
    public DateTime LastChecked { get; set; }
}

public class ConnectionTestResult
{
    public bool IsConnected { get; set; }
    public bool IsValid { get; set; }
    public DateTime TestedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SignatureGenerationResult
{
    public bool Success { get; set; }
    public string AuthorityName { get; set; } = string.Empty;
    public string AuthorityDesignation { get; set; } = string.Empty;
    public int SignatureSize { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class PROXKeyValidationResult
{
    public bool IsConnected { get; set; }
    public bool IsValid { get; set; }
    public DateTime ValidatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class DiagnosticsResult
{
    public DateTime Timestamp { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public double SuccessRate { get; set; }
    public List<DiagnosticTest> Tests { get; set; } = new();
}

public class DiagnosticTest
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Duration { get; set; }
}
