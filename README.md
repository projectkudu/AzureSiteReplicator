Azure Site Replicator
===================

Azure Site Extension to replicate the content of one site to other sites using msdeploy.

Note: when running on a local machine, it may be necessary to delete the reg key `HKLM\Software\Wow6432Node\Microsoft\IIS Extensions\msdeploy\3\extensibility`, to avoid
running into the issue described [here](http://serverfault.com/questions/524848/msbuild-failing-on-build-looking-for-older-version-of-microsoft-data-tools-schem).

## Setting up in an Azure Web Site

The following steps should get you set up:

- The site needs to be in Standard Mode so it supports Always On
- Always On needs to be enabled in the configuration page in the Azure portal. Please do this **before** installing Site Replicator (to get around a bug)
- Then go to the Site Extension gallery on the scm site, and install Site Replicator
- Restart the site using the button on the Site Extension gallery