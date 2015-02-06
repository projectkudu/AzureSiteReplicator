Azure Site Replicator
===================

Azure Site Extension to replicate the content of one site to other sites using `msdeploy`. The replication happens automatically when it detects changes within the `site/wwwroot` folder of the source site.


**Note**: when running on a local machine, it may be necessary to delete the reg key `HKLM\Software\Wow6432Node\Microsoft\IIS Extensions\msdeploy\3\extensibility`, to avoid
running into the issue described [here](http://serverfault.com/questions/524848/msbuild-failing-on-build-looking-for-older-version-of-microsoft-data-tools-schem).

## Setting up in an Azure Web Site

1. The website should be in **Standard Plan Mode**.
2. **Always On** needs to be enabled in the configuration page in the Azure portal. Please do this **before** installing Site Replicator (to get around a bug).
3. Open the **scm** site: [http://yourWebsite.scm.azurewebsites.net](http://<yourWebsite>.scm.azurewebsites.net)
4. Go to the **Site Extension Gallery**. 
5. Install **Site Replicator**.
6. Restart the website.

