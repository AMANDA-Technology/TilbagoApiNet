/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Shouldly;
using TilbagoApiNet.TestHelpers;

namespace TilbagoApiNet.E2eTests;

/// <summary>
///     End-to-end tests covering the case-creation flow against the live Tilbago API: create a case, query its status
///     and upload an attachment, for both natural-person and legal-person debtors.
/// </summary>
[TestFixture]
public class CreateCaseTests : E2eTestBase
{
    private const string FileName = "dummy.pdf";

    /// <summary>
    ///     Creates a natural-person case, queries its status and uploads an attachment, asserting each call returns a
    ///     non-null identifier or status payload.
    /// </summary>
    [Test]
    public async Task GetCreatedCaseAndAddAttachmentForNaturalPerson()
    {
        var view = TilbagoFakers.NaturalCaseFaker.Generate();

        var caseId = await TilbagoApiClient.CaseService.CreateNaturalPersonCaseAsync(view);
        caseId.ShouldNotBeNullOrWhiteSpace();
        RegisterCleanup(() =>
        {
            // Tilbago API does not expose a case-deletion endpoint (see ICaseService) — created cases must be
            // removed manually by the test environment owner. Cleanup is intentionally a no-op.
            return Task.CompletedTask;
        });

        var status = await TilbagoApiClient.CaseService.GetStatusAsync(caseId);
        status.ShouldNotBeNull();

        await using var fileStream = File.OpenRead(FileName);
        var attachmentId = await TilbagoApiClient.CaseService.AddAttachmentAsync(caseId, FileName, fileStream);
        attachmentId.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    ///     Creates a legal-person case using a fully populated <see cref="Abstractions.Views.DebtorLegalPersonView" />
    ///     payload, queries its status and uploads an attachment, asserting each call returns a non-null identifier or
    ///     status payload.
    /// </summary>
    [Test]
    public async Task GetCreatedCaseAndAddAttachmentForLegalPerson()
    {
        var view = TilbagoFakers.LegalCaseFaker.Generate();

        var caseId = await TilbagoApiClient.CaseService.CreateLegalPersonCaseAsync(view);
        caseId.ShouldNotBeNullOrWhiteSpace();
        RegisterCleanup(() =>
        {
            // Tilbago API does not expose a case-deletion endpoint (see ICaseService) — created cases must be
            // removed manually by the test environment owner. Cleanup is intentionally a no-op.
            return Task.CompletedTask;
        });

        var status = await TilbagoApiClient.CaseService.GetStatusAsync(caseId);
        status.ShouldNotBeNull();

        await using var fileStream = File.OpenRead(FileName);
        var attachmentId = await TilbagoApiClient.CaseService.AddAttachmentAsync(caseId, FileName, fileStream);
        attachmentId.ShouldNotBeNullOrWhiteSpace();
    }
}
