// <copyright file="IUFEmailBuilderService.cs" company="Ultra Force Development">
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
/// A helper interface to create and send an email.
/// </summary>
public interface IUFEmailBuilderService
{
  /// <summary>
  /// Start a new email, remove any previous stored data.
  /// </summary>
  /// <returns></returns>
  IUFEmailBuilderService Start();
  
  /// <summary>
  /// Sets the email subject.
  /// </summary>
  /// <param name="aSubject"></param>
  /// <returns></returns>
  IUFEmailBuilderService Subject(string aSubject);
  
  /// <summary>
  /// Sets the email sender.
  /// </summary>
  /// <param name="aFromEmail"></param>
  /// <param name="aName"></param>
  /// <returns></returns>
  IUFEmailBuilderService From(string aFromEmail, string? aName = null);

  /// <summary>
  /// Adds an email recipient.
  /// </summary>
  /// <param name="aToEmail"></param>
  /// <param name="aName"></param>
  /// <returns></returns>
  IUFEmailBuilderService To(string aToEmail, string? aName = null);

  /// <summary>
  /// Adds multiple email recipients.
  /// </summary>
  /// <param name="aEmailWithNames"></param>
  /// <returns></returns>
  IUFEmailBuilderService To(IDictionary<string, string?> aEmailWithNames);
  
  /// <summary>
  /// Adds a cc recipient.
  /// </summary>
  /// <param name="aToEmail"></param>
  /// <param name="aName"></param>
  /// <returns></returns>
  IUFEmailBuilderService Cc(string aToEmail, string? aName = null);

  /// <summary>
  /// Adds multiple cc recipients.
  /// </summary>
  /// <param name="aEmailWithNames"></param>
  /// <returns></returns>
  IUFEmailBuilderService Cc(IDictionary<string, string?> aEmailWithNames);

  /// <summary>
  /// Adds a bcc recipient.
  /// </summary>
  /// <param name="aToEmail"></param>
  /// <param name="aName"></param>
  /// <returns></returns>
  IUFEmailBuilderService Bcc(string aToEmail, string? aName = null);

  /// <summary>
  /// Adds multiple bcc recipients.
  /// </summary>
  /// <param name="aEmailWithNames"></param>
  /// <returns></returns>
  IUFEmailBuilderService Bcc(IDictionary<string, string?> aEmailWithNames);
  
  /// <summary>
  /// Sets the reply to email.
  /// </summary>
  /// <param name="aReplyToEmail"></param>
  /// <param name="aName"></param>
  /// <returns></returns>
  IUFEmailBuilderService ReplyTo(string aReplyToEmail, string? aName = null);

  /// <summary>
  /// Sets the html content.
  /// </summary>
  /// <param name="aContent"></param>
  /// <returns></returns>
  IUFEmailBuilderService Html(string aContent);

  /// <summary>
  /// Sets the text content.
  /// </summary>
  /// <param name="aContent"></param>
  /// <returns></returns>
  IUFEmailBuilderService Text(string aContent);

  /// <summary>
  /// Adds an attachment.
  /// </summary>
  /// <param name="aName"></param>
  /// <param name="aContentTYpe"></param>
  /// <param name="aData"></param>
  /// <returns></returns>
  IUFEmailBuilderService Attachment(string aName, string aContentTYpe, BinaryData aData);

  /// <summary>
  /// Adds attachments of the same content type.
  /// </summary>
  /// <param name="aContentType"></param>
  /// <param name="anAttachments"></param>
  /// <returns></returns>
  IUFEmailBuilderService Attachments(string aContentType, IDictionary<string, BinaryData> anAttachments);

  /// <summary>
  /// Sends the email.
  /// </summary>
  /// <returns>Empty string if email was sent successfully, else an error message.</returns>
  Task<string> SendAsync();
}