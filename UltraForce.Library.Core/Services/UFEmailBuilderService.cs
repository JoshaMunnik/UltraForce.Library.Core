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
  public abstract IUFEmailBuilderService Subject(string aSubject);

  /// <inheritdoc />
  public abstract IUFEmailBuilderService From(string aFromEmail, string? aName = null);

  /// <inheritdoc />
  public abstract IUFEmailBuilderService To(string aToEmail, string? aName = null);

  /// <inheritdoc />
  public IUFEmailBuilderService To(IDictionary<string, string?> aEmailWithNames)
  {
    foreach (KeyValuePair<string, string?> emailWithName in aEmailWithNames)
    {
      this.To(emailWithName.Key, emailWithName.Value);
    }
    return this;
  }

  /// <inheritdoc />
  public abstract IUFEmailBuilderService Cc(string aToEmail, string? aName = null);

  /// <inheritdoc />
  public IUFEmailBuilderService Cc(IDictionary<string, string?> aEmailWithNames)
  {
    foreach (KeyValuePair<string, string?> emailWithName in aEmailWithNames)
    {
      this.Cc(emailWithName.Key, emailWithName.Value);
    }
    return this;
  }

  /// <inheritdoc />
  public abstract IUFEmailBuilderService Bcc(string aToEmail, string? aName = null);

  /// <inheritdoc />
  public IUFEmailBuilderService Bcc(IDictionary<string, string?> aEmailWithNames)
  {
    foreach (KeyValuePair<string, string?> emailWithName in aEmailWithNames)
    {
      this.Bcc(emailWithName.Key, emailWithName.Value);
    }
    return this;
  }

  /// <inheritdoc />
  public abstract IUFEmailBuilderService ReplyTo(string aReplyToEmail, string? aName = null);

  /// <inheritdoc />
  public abstract IUFEmailBuilderService Html(string aContent);

  /// <inheritdoc />
  public abstract IUFEmailBuilderService Text(string aContent);

  /// <inheritdoc />
  public abstract IUFEmailBuilderService Attachment(string aName, string aContentTYpe, BinaryData aData);

  /// <inheritdoc />
  public IUFEmailBuilderService Attachments(
    string aContentType, IDictionary<string, BinaryData> anAttachments
  )
  {
    foreach (KeyValuePair<string, BinaryData> attachment in anAttachments)
    {
      this.Attachment(attachment.Key, aContentType, attachment.Value);
    }
    return this;
  }

  /// <inheritdoc />
  public abstract Task<string> SendAsync(bool aWaitForCompletion);
}