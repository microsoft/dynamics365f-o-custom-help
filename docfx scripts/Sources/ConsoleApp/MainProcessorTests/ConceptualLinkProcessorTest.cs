using System;
using System.IO;
using System.Text;
using MainProcessor;
using Xunit;

namespace MainProcessorTests
{
    public class ConceptualLinkProcessorTest
    {
        [Theory]
        [InlineData(
            "[!include [banner](../includes/banner.md)]",
            "[!include [banner](../includes/banner.md)]")]
        [InlineData(
            "[!include [banner](/includes/banner.md)]",
            "[!include [banner](/includes/banner.md)]")]
        [InlineData(
            "[!include [banner](./toc.md)]",
            "[!include [banner](./toc.md)]")]
        [InlineData(
            "[banner](../includes/pre-release.md)",
            "[banner](../includes/pre-release.md)")]
        [InlineData(
            "[!include [banner]()]",
            "[!include [banner]()]")]
        [InlineData(
            "[Dokumentation für die AD FS Server-Kapazität](/windows-server/identity/ad-fs/design/planning-for-ad-fs-server-capacity?p=1)",
            "[Dokumentation für die AD FS Server-KapazitätExternal text](https://github.com/windows-server/identity/ad-fs/design/planning-for-ad-fs-server-capacity?p=1)")]
        [InlineData(
            "[Aspekte zur Service Fabric-Clusterkapazitätsplanung](/azure/service-fabric/service-fabric-cluster-capacity)",
            "[Aspekte zur Service Fabric-ClusterkapazitätsplanungExternal text](https://github.com/azure/service-fabric/service-fabric-cluster-capacity)")]
        [InlineData(
            "[Docs.microsoft.com](/dynamics365/)",
            "[Docs.microsoft.comExternal text](https://github.com/dynamics365/)")]
        [InlineData(
            "[release notes](/includes/release-notes.md)",
            "[release notes](/includes/release-notes.md)")]
        [InlineData(
            "[Neues oder Änderungen in Dynamics 365 for Finance and Operations, Enterprise Edition (Juli 2017)](/dynamics365/unified-operations/dev-itpro/get-started/whats-new-application-July-2017-update)",
            "[Neues oder Änderungen in Dynamics 365 for Finance and Operations, Enterprise Edition (Juli 2017)External text](https://github.com/dynamics365/unified-operations/dev-itpro/get-started/whats-new-application-July-2017-update)")]
        [InlineData(
            "[Dynamics 365 Data Integration](/common-data-service/entity-reference/dynamics-365-integration)",
            "[Dynamics 365 Data IntegrationExternal text](https://github.com/common-data-service/entity-reference/dynamics-365-integration)")]
        [InlineData(
            "[Anpassung: Überlagerungen und Erweiterungen](../extensibility/customization-overlayering-extensions.md)",
            "[Anpassung: Überlagerungen und Erweiterungen](../extensibility/customization-overlayering-extensions.md)")]
        [InlineData(
            @"<a href=""/issue-search-lcs.md""><span style=""color: #0066cc;"">Problemsuche (Lifecycle Services, LCS)</a>",
            @"<a href=""/issue-search-lcs.md""><span style=""color: #0066cc;"">Problemsuche (Lifecycle Services, LCS)</a>")]
        [InlineData(
            @"<a href=""/deployment/deploy-demo-environment.md"">Eine Demoumgebung bereitstellen</a>",
            @"<a href=""/deployment/deploy-demo-environment.md"">Eine Demoumgebung bereitstellen</a>")]
        [InlineData(
            @"<td>In <a href=""https://lcs.dynamics.com/""><span style=""color: #0066cc;"">LCS</span></a> können Sie die Problemsuche verwenden, um Microsoft Wissensartikel, Hotfixes und Problemumgehungen für gemeldete Probleme in Finance and Operations zu finden. Sie können sehen, welche gemeldeten Probleme korrigiert werden oder schon korrigiert wurden in einem bestimmten Funktionsbereich. Weitere Informationen finden Sie unter <a href=""/issue-search-lcs.md""><span style=""color: #0066cc;"">Problemsuche (Lifecycle Services, LCS)</span></a>.</td>",
            @"<td>In <a href=""https://lcs.dynamics.com/""><span style=""color: #0066cc;"">LCS</span></a> können Sie die Problemsuche verwenden, um Microsoft Wissensartikel, Hotfixes und Problemumgehungen für gemeldete Probleme in Finance and Operations zu finden. Sie können sehen, welche gemeldeten Probleme korrigiert werden oder schon korrigiert wurden in einem bestimmten Funktionsbereich. Weitere Informationen finden Sie unter <a href=""/issue-search-lcs.md""><span style=""color: #0066cc;"">Problemsuche (Lifecycle Services, LCS)</span></a>.</td>")]
        [InlineData(
            @"<a href=""/media/ger-listoffields-function-format-output.png""><img src=""./media/ger-listoffields-function-format-output.png"" alt=""Format output"" class=""alignnone size-full wp-image-1204053"" width=""585"" height=""158"" /></a>",
            @"<a href=""/media/ger-listoffields-function-format-output.png""><img src=""./media/ger-listoffields-function-format-output.png"" alt=""Format output"" class=""alignnone size-full wp-image-1204053"" width=""585"" height=""158"" /></a>")]
        [InlineData(
            "8: [![Beispiel für die elektronische Berichterstellung](./media/electronic-reporting-example.png)](./media/electronic-reporting-example.png)",
            "8: [![Beispiel für die elektronische Berichterstellung](./media/electronic-reporting-example.png)](./media/electronic-reporting-example.png)")]
        [InlineData(
            "79: [![Finanzberichterstellungsbeispiel](./media/financial-reporting-example.png)](./media/financial-reporting-example.png)",
            "79: [![Finanzberichterstellungsbeispiel](./media/financial-reporting-example.png)](./media/financial-reporting-example.png)")]
        [InlineData(
            "79: [![Finanzberichterstellungsbeispiel](./media/111.png)](./media/222.png)",
            "79: [![Finanzberichterstellungsbeispiel](./media/111.png)](./media/222.png)")]
        public void Test_ProcessContentLinks_Processes_Links(string input, string expected)
        {
           StringBuilder sb = new StringBuilder();

            ConceptualLinkProcessor processor = new ConceptualLinkProcessor(new MockLogger(), 
                                                                            Path.GetTempPath(), 
                                                                            "https://github.com//de-de", 
                                                                            "https://github.com/en-us", 
                                                                            "https://github.com", 
                                                                            "External text",
                                                                            "toc.md", 
                                                                            null, 
                                                                            input,
                                                                            sb);

            bool shouldHaveChanges = !string.Equals(input, expected, StringComparison.InvariantCultureIgnoreCase);
            bool hasChanges = processor.ProcessContentLinks();
            Assert.Equal(shouldHaveChanges, hasChanges);

            Assert.Equal(expected, sb.ToString());
        }

        private class MockLogger : ILogger
        {
            public void LogInfo(string message = null, bool newLine = true)
            {
                //do nothing
            }

            public void LogWarning(string message = null, bool newLine = true)
            {
                //do nothing
            }

            public void LogError(string message = null, bool newLine = true)
            {
                //do nothing
            }

            public string GetLogContent()
            {
                return string.Empty;
            }
        }
    }
}
