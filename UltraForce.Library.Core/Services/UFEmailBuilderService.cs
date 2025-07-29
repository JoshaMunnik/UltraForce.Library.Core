// <copyright file="UFEmailBuilderService.cs" company="Ultra Force Development">
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

namespace UltraForce.Library.Core.Services;

/// <summary>
/// A base implementation of <see cref="IUFEmailBuilderService"/>. It implements the methods for
/// multiple entries.
/// </summary>
public abstract class UFEmailBuilderService : IUFEmailBuilderService
{
  /// <inheritdoc />
  public abstract IUFEmailBuilderService Start();

  /// <inheritdoc />
  public abstract IUFEmailBuilderService Subject(
    string subject
  );

  /// <inheritdoc />
  public abstract IUFEmailBuilderService From(
    string fromEmail,
    string? name = null
  );

  /// <inheritdoc />
  public abstract IUFEmailBuilderService To(
    string toEmail,
    string? name = null
  );

  /// <inheritdoc />
  public IUFEmailBuilderService To(
    IDictionary<string, string?> emailWithNames
  )
  {
    foreach (KeyValuePair<string, string?> emailWithName in emailWithNames)
    {
      this.To(emailWithName.Key, emailWithName.Value);
    }
    return this;
  }

  /// <inheritdoc />
  public abstract IUFEmailBuilderService Cc(
    string toEmail,
    string? name = null
  );

  /// <inheritdoc />
  public IUFEmailBuilderService Cc(
    IDictionary<string, string?> emailWithNames
  )
  {
    foreach (KeyValuePair<string, string?> emailWithName in emailWithNames)
    {
      this.Cc(emailWithName.Key, emailWithName.Value);
    }
    return this;
  }

  /// <inheritdoc />
  public abstract IUFEmailBuilderService Bcc(
    string toEmail,
    string? name = null
  );

  /// <inheritdoc />
  public IUFEmailBuilderService Bcc(
    IDictionary<string, string?> emailWithNames
  )
  {
    foreach (KeyValuePair<string, string?> emailWithName in emailWithNames)
    {
      this.Bcc(emailWithName.Key, emailWithName.Value);
    }
    return this;
  }

  /// <inheritdoc />
  public abstract IUFEmailBuilderService ReplyTo(
    string replyToEmail,
    string? name = null
  );

  /// <inheritdoc />
  public abstract IUFEmailBuilderService Html(
    string content
  );

  /// <inheritdoc />
  public abstract IUFEmailBuilderService Text(
    string content
  );

  /// <inheritdoc />
  public abstract IUFEmailBuilderService Attachment(
    string name,
    string contentTYpe,
    BinaryData data
  );

  /// <inheritdoc />
  public IUFEmailBuilderService Attachments(
    string contentType,
    IDictionary<string, BinaryData> attachments
  )
  {
    foreach (KeyValuePair<string, BinaryData> attachment in attachments)
    {
      this.Attachment(attachment.Key, contentType, attachment.Value);
    }
    return this;
  }

  /// <inheritdoc />
  public abstract Task<string> SendAsync(
    bool waitForCompletion
  );
}