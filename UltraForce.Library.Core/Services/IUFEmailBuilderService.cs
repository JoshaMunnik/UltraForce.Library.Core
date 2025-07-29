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
  /// <param name="subject"></param>
  /// <returns></returns>
  IUFEmailBuilderService Subject(
    string subject
  );

  /// <summary>
  /// Sets the email sender.
  /// </summary>
  /// <param name="fromEmail"></param>
  /// <param name="name"></param>
  /// <returns></returns>
  IUFEmailBuilderService From(
    string fromEmail,
    string? name = null
  );

  /// <summary>
  /// Adds an email recipient.
  /// </summary>
  /// <param name="toEmail"></param>
  /// <param name="name"></param>
  /// <returns></returns>
  IUFEmailBuilderService To(
    string toEmail,
    string? name = null
  );

  /// <summary>
  /// Adds multiple email recipients.
  /// </summary>
  /// <param name="emailWithNames"></param>
  /// <returns></returns>
  IUFEmailBuilderService To(
    IDictionary<string, string?> emailWithNames
  );

  /// <summary>
  /// Adds a cc recipient.
  /// </summary>
  /// <param name="toEmail"></param>
  /// <param name="name"></param>
  /// <returns></returns>
  IUFEmailBuilderService Cc(
    string toEmail,
    string? name = null
  );

  /// <summary>
  /// Adds multiple cc recipients.
  /// </summary>
  /// <param name="emailWithNames"></param>
  /// <returns></returns>
  IUFEmailBuilderService Cc(
    IDictionary<string, string?> emailWithNames
  );

  /// <summary>
  /// Adds a bcc recipient.
  /// </summary>
  /// <param name="toEmail"></param>
  /// <param name="name"></param>
  /// <returns></returns>
  IUFEmailBuilderService Bcc(
    string toEmail,
    string? name = null
  );

  /// <summary>
  /// Adds multiple bcc recipients.
  /// </summary>
  /// <param name="emailWithNames"></param>
  /// <returns></returns>
  IUFEmailBuilderService Bcc(
    IDictionary<string, string?> emailWithNames
  );

  /// <summary>
  /// Sets the reply to email.
  /// </summary>
  /// <param name="replyToEmail"></param>
  /// <param name="name"></param>
  /// <returns></returns>
  IUFEmailBuilderService ReplyTo(
    string replyToEmail,
    string? name = null
  );

  /// <summary>
  /// Sets the html content.
  /// </summary>
  /// <param name="content"></param>
  /// <returns></returns>
  IUFEmailBuilderService Html(
    string content
  );

  /// <summary>
  /// Sets the text content.
  /// </summary>
  /// <param name="content"></param>
  /// <returns></returns>
  IUFEmailBuilderService Text(
    string content
  );

  /// <summary>
  /// Adds an attachment.
  /// </summary>
  /// <param name="name"></param>
  /// <param name="contentTYpe"></param>
  /// <param name="data"></param>
  /// <returns></returns>
  IUFEmailBuilderService Attachment(
    string name,
    string contentTYpe,
    BinaryData data
  );

  /// <summary>
  /// Adds attachments of the same content type.
  /// </summary>
  /// <param name="contentType"></param>
  /// <param name="attachments"></param>
  /// <returns></returns>
  IUFEmailBuilderService Attachments(
    string contentType,
    IDictionary<string, BinaryData> attachments
  );

  /// <summary>
  /// Sends the email.
  /// </summary>
  /// <param name="waitForCompletion">
  /// True to wait until the email has been sent, false to return after the sending process has
  /// started. When set to false, the result of the send operation is unknown and the method
  /// will return an empty string if everything else was successful.
  /// </param>
  /// <returns>Empty string if email was sent successfully, else an error message.</returns>
  Task<string> SendAsync(
    bool waitForCompletion
  );
}
