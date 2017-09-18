# xRM Portals Community Edition

xRM Portals Community Edition is a fork of the open sourced version of [Microsoft Dynamics 365 Customer Engagement Portals](https://docs.microsoft.com/en-us/dynamics365/customer-engagement/portals/administer-manage-portal-dynamics-365). From a versioning standpoint it continues from the [announced one-time release of Portals source code](https://roadmap.dynamics.com/?i=4ba3b9c2-c92a-e711-80c0-00155d2433a1) made available on the [Microsoft Download Center](https://www.microsoft.com/en-us/download/details.aspx?id=55789) under the MIT license.

xRM Portals Community Edition enables portal deployments for Dynamics 365 online and on-premises environments, and allows developers to customize the code to suit their specific business needs.

## Objectives

xRM Portals Community Edition is a project led by [Adoxio](https://www.adoxio.com/) with contributions by the community to provide a way of upgrading from [Adxstudio Portals 7](https://community.adxstudio.com/products/adxstudio-portals/releases/adxstudio-portals-7/) to a supported version of portals, and that allows for the ability to migrate over to Microsoft's hosted offering of [Microsoft Dynamics 365 Customer Engagement Portals](https://docs.microsoft.com/en-us/dynamics365/customer-engagement/portals/administer-manage-portal-dynamics-365).

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

**Note:** If you are publishing `MasterPortal` to an IIS server that is running IIS 7.5, you will need to install the [IIS Application Initialization module](https://www.iis.net/downloads/microsoft/application-initialization) if it is not already installed. Use the appropriate download link at the [bottom of the page](https://www.iis.net/downloads/microsoft/application-initialization#additionalDownloads) as the *Install this extension* link at the top does not work.

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
