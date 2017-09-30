using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Portal.Web;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Adxstudio.Xrm")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
//[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Adxstudio.Xrm")]
//[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6cbc9827-3274-48db-8eea-56db80b234e7")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("5.0.0000")]
//[assembly: AssemblyVersion("5.1.0000")]
//[assembly: AssemblyFileVersion("5.1.0000")]

[assembly: EmbeddedResourceAssembly("^" + SiteSettings.XrmFilesRootPath + "/.*", "Adxstudio.Xrm.Files", "Adxstudio.Xrm.Files")]

[assembly: InternalsVisibleTo("Microsoft.Xrm.Portals.UnitTests, PublicKey=" +
"0024000004800000940000000602000000240000525341310004000001000100c9d8f61f7e87f4" +
"714e94e98f2e7d8483a59d88481f564ac27e4dc4523aae9bf59b8b347b73bb39de0e83b6b16cc9" +
"70926d9129cf4f5a7eaa989fa45b640e8bb77d141df0666bbdbe8bad52a6f8acdfbcd05494a772" +
"48187c7c6b9c9018d431886ebfcc744834ed447b117b1fd4d2fb83bb2f44e09ee0daf67db9c623" +
"52c55fa7")]
