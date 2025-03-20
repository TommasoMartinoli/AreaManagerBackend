using System.Collections.Generic;

public class EmailRequest
{
    public required string EmailSender { get; set; }
    public required string EmailReceiver { get; set; }
    public string? EmailCC { get; set; }
    public string? EmailObject { get; set; }
    public string? EmailBody { get; set; }
    public string? EmailReplyTo { get; set; }
    public required List<EmailAttachment> Attachments { get; set; }
}

public class EmailAttachment
{
    public required string Name { get; set; } 
    public required string Content { get; set; } 
}
