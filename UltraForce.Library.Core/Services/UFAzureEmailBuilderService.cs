// <copyright file="UFAzureEmailBuilderService.cs" company="Ultra Force Development">
// Ultra Force Library - Copyright (C) 2024 Ultra Force Development
// </copyright>
// <author>Josha Munnik</author>
// <email>josha@ultraforce.com</email>
// <license>
// The MIT License (MIT)
//
// Copyright (C) 2024 Ultra Force Development
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
// </license>

using Azure;
using Azure.Communication.Email;

namespace UltraForce.Library.Core.Services;

/// <summary>
/// An implement of <see cref="IUFEmailBuilderService"/> using Azure's Email Communication Resource.
/// <para>
/// Subclasses must implement <see cref="GetConnectionString"/>. Subclasses can also override
/// <see cref="GetFromEmail"/> with an email address set within the communication resource.
/// </para>
/// </summary>
public abstract class UFAzureEmailBuilderService : UFEmailBuilderService
{
  #region private variables

  /// <summary>
  /// Required
  /// </summary>
  private EmailAddress? m_from = null;

  /// <summary>
  /// Required
  /// </summary>
  private EmailContent? m_content = null;

  /// <summary>
  /// Used to temporary store the html content before <see cref="m_content"/> is set.
  /// </summary>
  private string? m_html = null;

  /// <summary>
  /// Used to temporary store the text content before <see cref="m_content"/> is set.
  /// </summary>
  private string? m_text = null;

  /// <summary>
  /// Should be at least one recipient.
  /// </summary>
  private readonly List<EmailAddress> m_to = [];

  /// <summary>
  /// Should be at least one recipient.
  /// </summary>
  private readonly List<EmailAddress> m_cc = [];

  /// <summary>
  /// Should be at least one recipient.
  /// </summary>
  private readonly List<EmailAddress> m_bcc = [];

  /// <summary>
  /// Optional reply to email address.
  /// </summary>
  private EmailAddress? m_replyTo = null;

  /// <summary>
  /// Attachments.
  /// </summary>
  private readonly List<EmailAttachment> m_attachments = [];

  /// <summary>
  /// Share email client between instances.
  /// </summary>
  private static EmailClient? s_emailClient = null;

  /// <summary>
  /// Used when creating the email client
  /// </summary>
  private static readonly object s_emailClientLock = new object();

  #endregion

  #region IUFEmailBuilder

  /// <inheritdoc />
  public override IUFEmailBuilderService Start()
  {
    this.m_from = null;
    this.m_content = null;
    this.m_html = null;
    this.m_text = null;
    this.m_to.Clear();
    this.m_cc.Clear();
    this.m_bcc.Clear();
    this.m_replyTo = null;
    this.m_attachments.Clear();
    return this;
  }

  /// <inheritdoc />
  public override IUFEmailBuilderService Subject(
    string subject
  )
  {
    this.m_content = new EmailContent(subject);
    if (this.m_html != null)
    {
      this.m_content.Html = this.m_html;
    }
    if (this.m_text != null)
    {
      this.m_content.PlainText = this.m_text;
    }
    return this;
  }

  /// <inheritdoc />
  public override IUFEmailBuilderService From(
    string fromEmail,
    string? name = null
  )
  {
    this.m_from = CreateEmailAddress(fromEmail, name);
    return this;
  }

  /// <inheritdoc />
  public override IUFEmailBuilderService To(
    string toEmail,
    string? name = null
  )
  {
    this.m_to.Add(CreateEmailAddress(toEmail, name));
    return this;
  }

  /// <inheritdoc />
  public override IUFEmailBuilderService Cc(
    string toEmail,
    string? name = null
  )
  {
    this.m_cc.Add(CreateEmailAddress(toEmail, name));
    return this;
  }

  /// <inheritdoc />
  public override IUFEmailBuilderService Bcc(
    string toEmail,
    string? name = null
  )
  {
    this.m_bcc.Add(CreateEmailAddress(toEmail, name));
    return this;
  }

  /// <inheritdoc />
  public override IUFEmailBuilderService ReplyTo(
    string replyToEmail,
    string? name = null
  )
  {
    this.m_replyTo = CreateEmailAddress(replyToEmail, name);
    return this;
  }

  /// <inheritdoc />
  public override IUFEmailBuilderService Html(
    string content
  )
  {
    if (this.m_content != null)
    {
      this.m_content.Html = content;
    }
    else
    {
      this.m_html = content;
    }
    return this;
  }

  /// <inheritdoc />
  public override IUFEmailBuilderService Text(
    string content
  )
  {
    if (this.m_content != null)
    {
      this.m_content.PlainText = content;
    }
    else
    {
      this.m_text = content;
    }
    return this;
  }

  /// <inheritdoc />
  public override IUFEmailBuilderService Attachment(
    string name,
    string contentTYpe,
    BinaryData data
  )
  {
    this.m_attachments.Add(new EmailAttachment(name, contentTYpe, data));
    return this;
  }

  /// <inheritdoc />
  public override async Task<string> SendAsync(
    bool waitForCompletion
  )
  {
    string fromEmail = this.m_from?.Address ?? this.GetFromEmail();
    if (string.IsNullOrWhiteSpace(fromEmail))
    {
      throw new Exception("Missing from");
    }
    if (this.m_content == null)
    {
      throw new Exception("Missing content");
    }
    if (this.m_to.Count == 0)
    {
      throw new Exception("Missing recipients");
    }
    EmailRecipients recipients = new(this.m_to, this.m_cc, this.m_bcc);
    EmailMessage message = new(
      fromEmail,
      recipients,
      this.m_content
    );
    if (this.m_replyTo != null)
    {
      message.ReplyTo.Add(this.m_replyTo!.Value);
    }
    foreach (EmailAttachment attachment in this.m_attachments)
    {
      message.Attachments.Add(attachment);
    }
    try
    {
      EmailClient emailClient = GetEmailClient(this.GetConnectionString());
      EmailSendOperation sendOperation = await emailClient.SendAsync(
        waitForCompletion ? WaitUntil.Completed : WaitUntil.Started, message
      );
      if (waitForCompletion)
      {
        // get the OperationId so that it can be used for tracking the message for troubleshooting
        string operationId = sendOperation.Id;
        return sendOperation.Value.Status == EmailSendStatus.Succeeded
          ? ""
          : $"Failed: {sendOperation.Value.Status}, operation id: {operationId}";
      }
      return "";
    }
    catch (RequestFailedException ex)
    {
      // operationID is contained in the exception message and can be used for
      // troubleshooting purposes
      return $"Exception: {ex.ErrorCode}, message: {ex.Message}";
    }
  }

  #endregion

  #region abstract methods

  /// <summary>
  /// Gets the connection string for the email service.
  /// </summary>
  /// <returns></returns>
  protected abstract string GetConnectionString();

  /// <summary>
  /// Returns the email address to use as the sender. Azure does not support names for the from
  /// address.
  /// </summary>
  /// <returns></returns>
  protected virtual string GetFromEmail()
  {
    return "";
  }

  #endregion

  #region private methods

  /// <summary>
  /// Creates an email address with optional name (if any).
  /// </summary>
  /// <param name="email"></param>
  /// <param name="name"></param>
  /// <returns></returns>
  private static EmailAddress CreateEmailAddress(
    string email,
    string? name
  )
  {
    return (name == null)
      ? new EmailAddress(email)
      : new EmailAddress(email, name);
  }

  /// <summary>
  /// Gets the email client. With the first call the client is created, the client is cached
  /// and shared between instances.
  /// </summary>
  /// <param name="connectionString"></param>
  /// <returns></returns>
  private static EmailClient GetEmailClient(
    string connectionString
  )
  {
    if (s_emailClient != null)
    {
      return s_emailClient;
    }
    lock (s_emailClientLock)
    {
      s_emailClient ??= new EmailClient(connectionString);
    }
    return s_emailClient;
  }

  #endregion
}
