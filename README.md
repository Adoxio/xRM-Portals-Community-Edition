# xRM Portals Community Edition

xRM Portals Community Edition is a fork of the open source release of [Portal Capabilities for Microsoft Dynamics 365](https://docs.microsoft.com/en-us/dynamics365/customer-engagement/portals/administer-manage-portal-dynamics-365) version 8.3. It continues from the [announced one-time release of Portals source code](https://roadmap.dynamics.com/?i=e2f80f10-118c-e711-8118-3863bb36dd08#) made available on the [Microsoft Download Center](https://www.microsoft.com/en-us/download/details.aspx?id=55789) under the MIT license.

xRM Portals Community Edition enables portal deployments for Dynamics 365 online and on-premises environments, and allows developers to customize the code to suit their specific business needs.

## Objectives

xRM Portals Community Edition is a project led by [Adoxio](https://www.adoxio.com/) with contributions by the community to provide a way of upgrading from [Adxstudio Portals 7](https://community.adxstudio.com/products/adxstudio-portals/releases/adxstudio-portals-7/) to this supported version of portals, and that provides a migration path to Microsoft's hosted offering of [Portal Capabilities for Microsoft Dynamics 365](https://docs.microsoft.com/en-us/dynamics365/customer-engagement/portals/administer-manage-portal-dynamics-365).

This version is locked to the features as of the 8.3 version of code that Microsoft released. Maintaining feature parity with Microsoft's version is not an objective for this project, and new features that Microsoft adds to the online version are not going to be implemented in this project. At present, changes are focused on bug fixes and general supportability.
 
**New portal implementations should use Microsoft's software as a service version. This project is primarily intended for those who are already using Adxstudio Portals and want to perform the relatively smaller efforts needed to use this supported project while taking more time to migrate their existing applications to [Portal Capabilities for Microsoft Dynamics 365](https://docs.microsoft.com/en-us/dynamics365/customer-engagement/portals/administer-manage-portal-dynamics-365).**

Using Microsoft's online version should be the primary goal of existing and new users, and using this version is primarily intended for those with special circumstances where they need to stay on premise for a longer time period while preparing to move online. We understand that there will be varying reasons to use this version, but Microsoft's offering is the recommended long term solution that we recommend.

## Building

To build the project, ensure that you have [Git](https://git-scm.com/downloads) installed to obtain the source code, and [Visual Studio 2017](https://docs.microsoft.com/en-us/visualstudio/welcome-to-visual-studio) installed to compile the source code.

- Clone the repository using Git:
  ```sh
  git clone https://github.com/Adoxio/xRM-Portals-Community-Edition.git
  ```
- Open the `Solutions\Portals\Portals.sln` solution file in Visual Studio
- Build the `Portals` solution or the `MasterPortal` project in Visual Studio

## Deployment

xRM Portals Community Edition is a set of .NET class libraries and an ASP.NET web application called `MasterPortal`. After building the project, `MasterPortal` is run using conventional ASP.NET website hosting methods such as using [IIS](https://www.iis.net/) in on-premise environments, and [Azure Web Apps](https://docs.microsoft.com/en-ca/azure/app-service-web/app-service-web-overview) in cloud environments.

The `MasterPortal` web application  deployment is dependent upon schema (solutions) and data being installed in a Dynamics 365 instance. These components are downloaded from the [Microsoft Download Center](https://www.microsoft.com/en-us/download/details.aspx?id=55789) in the file `MicrosoftDynamics365PortalsSolutions.exe`. The components in this download have not been released under the MIT license and are not managed by the xRM Portals Community Edition project.

A full description of the deployment process is described in the file `Self-hosted_Installation_Guide_for_Portals.pdf` available for download on the [Microsoft Download Center](https://www.microsoft.com/en-us/download/details.aspx?id=55789).

## System Requirements

The following system requirements are additional to those listed in `Self-hosted_Installation_Guide_for_Portals.pdf`:

- .NET Framework 4.7 must be installed ([download](https://www.microsoft.com/net/download/dotnet-framework-runtime/net47), [system requirements](https://docs.microsoft.com/en-us/dotnet/framework/get-started/system-requirements)).

- The website must be set to run in 64-bit mode:

  IIS Application Pool:
   
  ![image](https://user-images.githubusercontent.com/10599498/30821566-03ec5466-a1e3-11e7-80bd-bb0b1c724452.png)

  Azure Web App:
   
  ![image](https://user-images.githubusercontent.com/10599498/30821633-468576ae-a1e3-11e7-8b45-e55df1742629.png)

- IIS 7.5 (Windows 7 or Windows Server 2008 R2) requires the installation of the [IIS Application Initialization module](https://www.iis.net/downloads/microsoft/application-initialization). Use the `x64` download link at the [bottom of the page](https://www.iis.net/downloads/microsoft/application-initialization#additionalDownloads).

- TLS 1.2 needs to be enabled on older operating systems when connecting to Dynamics 365 CE Online 9.0. Refer to the [Enable TLS 1.2 and 1.1 support on older operating systems](https://github.com/Adoxio/xRM-Portals-Community-Edition/wiki/Enable-TLS-1.2-and-1.1-support-on-older-operating-systems) wiki page for full instructions.

- File system permissions need to be set for general functionality and search indexing to work. Refer to the [File System Permissions](https://github.com/Adoxio/xRM-Portals-Community-Edition/wiki/File-System-Permissions) wiki page for full instructions.

## Support

There are two primary methods of obtaining support for this project:

1. Community-driven support is available by [submitting issues](https://github.com/Adoxio/xRM-Portals-Community-Edition/issues) to this GitHub project
2. Commercial support options are available from [Adoxio](https://www.adoxio.com/xRM-Portals-Community-Edition/)

## License

This project uses the [MIT license](https://opensource.org/licenses/MIT).

## Contributions

This project accepts community contributions through GitHub, following the [inbound=outbound](https://opensource.guide/legal/#does-my-project-need-an-additional-contributor-agreement) model as described in the [GitHub Terms of Service](https://help.github.com/articles/github-terms-of-service/#6-contributions-under-repository-license):
> Whenever you make a contribution to a repository containing notice of a license, you license your contribution under the same terms, and you agree that you have the right to license your contribution under those terms.

Please submit one pull request per issue so that we can easily identify and review the changes.
