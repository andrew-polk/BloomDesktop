#!/bin/bash
# server=build.palaso.org
# project=Bloom
# build=Bloom-4.7-Linux64-Continuous
# root_dir=..
# Auto-generated by https://github.com/chrisvire/BuildUpdate.
# Do not edit this file by hand!

cd "$(dirname "$0")"

# *** Functions ***
force=0
clean=0

while getopts fc opt; do
case $opt in
f) force=1 ;;
c) clean=1 ;;
esac
done

shift $((OPTIND - 1))

copy_auto() {
if [ "$clean" == "1" ]
then
echo cleaning $2
rm -f ""$2""
else
where_curl=$(type -P curl)
where_wget=$(type -P wget)
if [ "$where_curl" != "" ]
then
copy_curl "$1" "$2"
elif [ "$where_wget" != "" ]
then
copy_wget "$1" "$2"
else
echo "Missing curl or wget"
exit 1
fi
fi
}

copy_curl() {
echo "curl: $2 <= $1"
if [ -e "$2" ] && [ "$force" != "1" ]
then
curl -# -L -z "$2" -o "$2" "$1"
else
curl -# -L -o "$2" "$1"
fi
}

copy_wget() {
echo "wget: $2 <= $1"
f1=$(basename $1)
f2=$(basename $2)
cd $(dirname $2)
wget -q -L -N "$1"
# wget has no true equivalent of curl's -o option.
# Different versions of wget handle (or not) % escaping differently.
# A URL query is the only reason why $f1 and $f2 should differ.
if [ "$f1" != "$f2" ]; then mv $f2\?* $f2; fi
cd -
}


# *** Results ***
# build: Bloom-4.7-Linux64-Continuous (Bloom_Bloom47Linux64Continuous)
# project: Bloom
# URL: https://build.palaso.org/viewType.html?buildTypeId=Bloom_Bloom47Linux64Continuous
# VCS: git://github.com/BloomBooks/BloomDesktop.git [Version4.7]
# dependencies:
# [0] build: bloom-win32-static-dependencies (bt396)
#     project: Bloom
#     URL: https://build.palaso.org/viewType.html?buildTypeId=bt396
#     clean: false
#     revision: bloom-4.7.tcbuildtag
#     paths: {"connections.dll"=>"DistFiles", "MSBuild.Community.Tasks.dll"=>"build/", "MSBuild.Community.Tasks.Targets"=>"build/"}
# [1] build: Bloom Help 4.7 (Bloom_Help_BloomHelp47)
#     project: Help
#     URL: https://build.palaso.org/viewType.html?buildTypeId=Bloom_Help_BloomHelp47
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.chm"=>"DistFiles"}
# [2] build: pdf.js (bt401)
#     project: BuildTasks
#     URL: https://build.palaso.org/viewType.html?buildTypeId=bt401
#     clean: false
#     revision: bloom-4.7.tcbuildtag
#     paths: {"pdfjs-viewer.zip!**"=>"DistFiles/pdf"}
#     VCS: https://github.com/mozilla/pdf.js.git [gh-pages]
# [3] build: GeckofxHtmlToPdf-xenial64-continuous (GeckofxHtmlToPdf_GeckofxHtmlToPdfXenial64continuous)
#     project: GeckofxHtmlToPdf
#     URL: https://build.palaso.org/viewType.html?buildTypeId=GeckofxHtmlToPdf_GeckofxHtmlToPdfXenial64continuous
#     clean: false
#     revision: bloom-4.7.tcbuildtag
#     paths: {"Args.dll"=>"lib/dotnet", "GeckofxHtmlToPdf.exe"=>"lib/dotnet", "GeckofxHtmlToPdf.exe.config"=>"lib/dotnet"}
#     VCS: https://github.com/BloomBooks/geckofxHtmlToPdf [refs/heads/master]
# [4] build: L10NSharp master Mono continuous (L10NSharp_L10NSharpMasterMonoContinuous)
#     project: L10NSharp
#     URL: https://build.palaso.org/viewType.html?buildTypeId=L10NSharp_L10NSharpMasterMonoContinuous
#     clean: false
#     revision: bloom-4.7.tcbuildtag
#     paths: {"L10NSharp.dll*"=>"lib/dotnet/", "CheckOrFixXliff.exe*"=>"lib/dotnet/"}
#     VCS: https://github.com/sillsdev/l10nsharp [refs/heads/master]
# [5] build: PdfDroplet-Linux-Dev-Continuous (bt344)
#     project: PdfDroplet
#     URL: https://build.palaso.org/viewType.html?buildTypeId=bt344
#     clean: false
#     revision: bloom-4.7.tcbuildtag
#     paths: {"PdfDroplet.exe"=>"lib/dotnet", "PdfSharp.dll*"=>"lib/dotnet"}
#     VCS: https://github.com/sillsdev/pdfDroplet [master]
# [6] build: TidyManaged-master-linux64-continuous (bt351)
#     project: TidyManaged
#     URL: https://build.palaso.org/viewType.html?buildTypeId=bt351
#     clean: false
#     revision: bloom-4.7.tcbuildtag
#     paths: {"TidyManaged.dll*"=>"lib/dotnet"}
#     VCS: https://github.com/BloomBooks/TidyManaged.git [master]
# [7] build: Linux master continuous (XliffForHtml_LinuxMasterContinuous)
#     project: XliffForHtml
#     URL: https://build.palaso.org/viewType.html?buildTypeId=XliffForHtml_LinuxMasterContinuous
#     clean: false
#     revision: bloom-4.7.tcbuildtag
#     paths: {"HtmlXliff.*"=>"lib/dotnet", "HtmlAgilityPack.*"=>"lib/dotnet"}
#     VCS: https://github.com/sillsdev/XliffForHtml [refs/heads/master]
# [8] build: palaso-linux64-master Continuous (Libpalaso_PalasoLinux64masterContinuous)
#     project: libpalaso
#     URL: https://build.palaso.org/viewType.html?buildTypeId=Libpalaso_PalasoLinux64masterContinuous
#     clean: false
#     revision: bloom-4.7.tcbuildtag
#     paths: {"Newtonsoft.Json.dll"=>"lib/dotnet/", "SIL.Core.dll*"=>"lib/dotnet/", "SIL.Core.pdb"=>"lib/dotnet/", "SIL.Core.Desktop.dll*"=>"lib/dotnet/", "SIL.Core.Desktop.pdb"=>"lib/dotnet/", "SIL.Media.dll*"=>"lib/dotnet/", "SIL.Media.pdb"=>"lib/dotnet/", "SIL.TestUtilities.dll*"=>"lib/dotnet/", "SIL.TestUtilities.pdb"=>"lib/dotnet/", "SIL.Windows.Forms.dll*"=>"lib/dotnet/", "SIL.Windows.Forms.pdb"=>"lib/dotnet/", "SIL.Windows.Forms.GeckoBrowserAdapter.dll*"=>"lib/dotnet/", "SIL.Windows.Forms.GeckoBrowserAdapter.pdb"=>"lib/dotnet/", "SIL.Windows.Forms.Keyboarding.dll*"=>"lib/dotnet/", "SIL.Windows.Forms.Keyboarding.pdb"=>"lib/dotnet/", "SIL.Windows.Forms.WritingSystems.dll*"=>"lib/dotnet/", "SIL.Windows.Forms.WritingSystems.pdb"=>"lib/dotnet/", "SIL.WritingSystems.dll*"=>"lib/dotnet/", "SIL.WritingSystems.pdb"=>"lib/dotnet/", "taglib-sharp.dll*"=>"lib/dotnet/", "Enchant.Net.dll*"=>"lib/dotnet/", "NDesk.DBus.dll*"=>"lib/dotnet/"}
#     VCS: https://github.com/sillsdev/libpalaso.git [refs/heads/master]

# make sure output directories exist
mkdir -p ../DistFiles
mkdir -p ../DistFiles/pdf
mkdir -p ../Downloads
mkdir -p ../build/
mkdir -p ../lib/dotnet
mkdir -p ../lib/dotnet/

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt396/bloom-4.7.tcbuildtag/connections.dll ../DistFiles/connections.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt396/bloom-4.7.tcbuildtag/MSBuild.Community.Tasks.dll ../build/MSBuild.Community.Tasks.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt396/bloom-4.7.tcbuildtag/MSBuild.Community.Tasks.Targets ../build/MSBuild.Community.Tasks.Targets
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Help_BloomHelp47/latest.lastSuccessful/Bloom.chm ../DistFiles/Bloom.chm
copy_auto http://build.palaso.org/guestAuth/repository/download/bt401/bloom-4.7.tcbuildtag/pdfjs-viewer.zip ../Downloads/pdfjs-viewer.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/GeckofxHtmlToPdf_GeckofxHtmlToPdfXenial64continuous/bloom-4.7.tcbuildtag/Args.dll ../lib/dotnet/Args.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/GeckofxHtmlToPdf_GeckofxHtmlToPdfXenial64continuous/bloom-4.7.tcbuildtag/GeckofxHtmlToPdf.exe ../lib/dotnet/GeckofxHtmlToPdf.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/GeckofxHtmlToPdf_GeckofxHtmlToPdfXenial64continuous/bloom-4.7.tcbuildtag/GeckofxHtmlToPdf.exe.config ../lib/dotnet/GeckofxHtmlToPdf.exe.config
copy_auto http://build.palaso.org/guestAuth/repository/download/L10NSharp_L10NSharpMasterMonoContinuous/bloom-4.7.tcbuildtag/L10NSharp.dll ../lib/dotnet/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/L10NSharp_L10NSharpMasterMonoContinuous/bloom-4.7.tcbuildtag/L10NSharp.dll.config ../lib/dotnet/L10NSharp.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/L10NSharp_L10NSharpMasterMonoContinuous/bloom-4.7.tcbuildtag/CheckOrFixXliff.exe ../lib/dotnet/CheckOrFixXliff.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/L10NSharp_L10NSharpMasterMonoContinuous/bloom-4.7.tcbuildtag/CheckOrFixXliff.exe.config ../lib/dotnet/CheckOrFixXliff.exe.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt344/bloom-4.7.tcbuildtag/PdfDroplet.exe ../lib/dotnet/PdfDroplet.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt344/bloom-4.7.tcbuildtag/PdfSharp.dll ../lib/dotnet/PdfSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt351/bloom-4.7.tcbuildtag/TidyManaged.dll ../lib/dotnet/TidyManaged.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt351/bloom-4.7.tcbuildtag/TidyManaged.dll.config ../lib/dotnet/TidyManaged.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/XliffForHtml_LinuxMasterContinuous/bloom-4.7.tcbuildtag/HtmlXliff.exe ../lib/dotnet/HtmlXliff.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/XliffForHtml_LinuxMasterContinuous/bloom-4.7.tcbuildtag/HtmlXliff.exe.mdb ../lib/dotnet/HtmlXliff.exe.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/XliffForHtml_LinuxMasterContinuous/bloom-4.7.tcbuildtag/HtmlAgilityPack.dll ../lib/dotnet/HtmlAgilityPack.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/Newtonsoft.Json.dll ../lib/dotnet/Newtonsoft.Json.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Core.dll ../lib/dotnet/SIL.Core.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Core.pdb ../lib/dotnet/SIL.Core.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Core.Desktop.dll ../lib/dotnet/SIL.Core.Desktop.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Core.Desktop.pdb ../lib/dotnet/SIL.Core.Desktop.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Media.dll ../lib/dotnet/SIL.Media.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Media.dll.config ../lib/dotnet/SIL.Media.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Media.pdb ../lib/dotnet/SIL.Media.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.TestUtilities.dll ../lib/dotnet/SIL.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.TestUtilities.pdb ../lib/dotnet/SIL.TestUtilities.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Windows.Forms.dll ../lib/dotnet/SIL.Windows.Forms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Windows.Forms.dll.config ../lib/dotnet/SIL.Windows.Forms.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Windows.Forms.pdb ../lib/dotnet/SIL.Windows.Forms.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Windows.Forms.GeckoBrowserAdapter.dll ../lib/dotnet/SIL.Windows.Forms.GeckoBrowserAdapter.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Windows.Forms.GeckoBrowserAdapter.pdb ../lib/dotnet/SIL.Windows.Forms.GeckoBrowserAdapter.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Windows.Forms.Keyboarding.dll ../lib/dotnet/SIL.Windows.Forms.Keyboarding.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Windows.Forms.Keyboarding.dll.config ../lib/dotnet/SIL.Windows.Forms.Keyboarding.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Windows.Forms.Keyboarding.pdb ../lib/dotnet/SIL.Windows.Forms.Keyboarding.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Windows.Forms.WritingSystems.dll ../lib/dotnet/SIL.Windows.Forms.WritingSystems.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.Windows.Forms.WritingSystems.pdb ../lib/dotnet/SIL.Windows.Forms.WritingSystems.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.WritingSystems.dll ../lib/dotnet/SIL.WritingSystems.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/SIL.WritingSystems.pdb ../lib/dotnet/SIL.WritingSystems.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/taglib-sharp.dll ../lib/dotnet/taglib-sharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/Enchant.Net.dll ../lib/dotnet/Enchant.Net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/Enchant.Net.dll.config ../lib/dotnet/Enchant.Net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/NDesk.DBus.dll ../lib/dotnet/NDesk.DBus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/bloom-4.7.tcbuildtag/NDesk.DBus.dll.config ../lib/dotnet/NDesk.DBus.dll.config
# extract downloaded zip files
unzip -uqo ../Downloads/pdfjs-viewer.zip -d "../DistFiles/pdf"
# End of script
