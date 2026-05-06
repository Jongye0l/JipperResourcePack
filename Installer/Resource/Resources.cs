using System;
using System.Globalization;

namespace JipperResourcePack.Installer.Resource;

public class Resources {
    public static readonly Resources English = new() {
        Title = "Jipper ResourcePack Installer",
        Cancel = "Cancel",
        Previous = "< Previous",
        Next = "Next >",
        MainScreen_Title = "The Installer starts the installation.",
        // MainScreen_Description = "Install the Jipper ResourcePack on the Adofai.\nAn Internet connection is required to do this.\nPress the 'Next' button to continue.",
        // TODO: Main Screen Translate
        MainScreen_NoInternet = "You are not connected to the Internet.",
        Error = "Error",
        SelectLocation_Title = "Please choose the location of Adofai.",
        SelectLocation_Description = "Install the Jipper ResourcePack in the Adofai.\nIf you install it in a different Adofai folder or if the program has not found the Adofai folder, click the 'Select Folder' button to select the Adofai folder.",
        SelectLocation_Select = "Select Folder",
        SelectLocation_AdofaiFolderGuide = "Adofai folder guide",
        SelectLocation_NoLocation = "The location is not set.",
        SelectLocation_NoFolder = "The folder does not exist.",
        SelectLocation_NoAdofai = "The selected folder is not an Adofai folder.",
        SelectMod_Title = "Please select additional mods to install.",
        SelectMod_Description = "Please select the mods you want to install.",
        SelectLocation_Location = "Adofai Folder",
        SelectMod_Mod = "Select Additional Mods",
        Install_Install = "Installing...",
        Install_Finish = "Exit",
        FinishScreen_Title = "Jipper ResourcePack Installation Completed",
        FinishScreen_Description = "Jipper ResourcePack installation completed.\nPress the 'Exit' button to finish the installation program.",
        FinishScreen_RunAdofai = "Run Adofai",
        FinishScreen_Title_Error = "Failed to install Jipper ResourcePack",
        FinishScreen_Description_Error = "An error occurred during the installation of the Jipper ResourcePack.\nPlease press the 'View Log' button to check for detailed errors.",
        FinishScreen_CheckLog = "View Log"
    };

    public static readonly Resources Korean = new() {
        Title = "지퍼리소스팩 딸깍 설치기",
        Cancel = "취소",
        Previous = "< 이전",
        Next = "다음 >",
        MainScreen_Title = "지퍼 리소스팩 설치를 시작합니다.",
        // MainScreen_Description = "얼불춤에 지퍼 리소스팩을 설치합니다.\n이 작업을 하기 위해서는 인터넷 연결이 필요합니다.\n계속 하시려면 '다음' 버튼을 눌러주세요.",
        MainScreen_Description1 = "불과 얼음의 춤에 지퍼 리소스팩을 설치합니다.",
        MainScreen_Description2 = "설치를 위해 github에 접속할 수 있는 환경이 필요합니다.",
        MainScreen_Description3 = "UnityModManager와 JALib등 필요한 시스템이 자동으로 설치될 수 있습니다.",
        MainScreen_Description4 = "지퍼 리소스팩은 최신 older, default, beta, alpha 브랜치만 지원하며\n그 외 버전에서는 작동하지 않을 수 있습니다.",
        MainScreen_Description5 = "설치 작업을 진행하면 불과 얼음의 춤이 강제 종료됩니다.",
        MainScreen_Description6 = "이 프로그램은 7th Beat Games 공식 프로그램이 아닌 커뮤니티 모드입니다.",
        MainScreen_Description7 = "계속 하시려면 '다음' 버튼을 눌러주세요.",
        MainScreen_BugReport = "버그/문의",
        MainScreen_Donate = "후원",
        MainScreen_KakaoPayQrForm = "지퍼 리소스팩 후원 카카오페이 QR",
        MainScreen_OpenBrowserFailed = "브라우저를 여는 데 실패했습니다. 클립보드로 URL이 복사되었습니다.",
        MainScreen_NoInternet = "인터넷에 연결되어 있지 않습니다.",
        Error = "오류",
        SelectLocation_Title = "얼불춤의 폴더를 선택해주세요.",
        SelectLocation_Description = "지퍼 리소스팩을 얼불춤에 설치합니다.\n다른 얼불춤 폴더에 설치하거나 얼불춤 폴더를 프로그램이 찾지 못했다면 '찾아보기' 버튼을 눌러 얼불춤 폴더를 선택해주세요.",
        SelectLocation_Select = "찾아보기",
        SelectLocation_AdofaiFolderGuide = "얼불춤 폴더 가이드",
        SelectLocation_NoLocation = "위치가 설정되지 않았습니다.",
        SelectLocation_NoFolder = "폴더가 존재하지 않습니다.",
        SelectLocation_NoAdofai = "선택된 폴더는 얼불춤 폴더가 아닙니다.",
        SelectMod_Title = "추가로 설치할 모드를 선택해주세요.",
        SelectMod_Description = "설치를 원하시는 모드를 선택해주세요.",
        SelectLocation_Location = "얼불춤 폴더",
        SelectMod_Mod = "추가 모드 선택",
        Install_Install = "설치중...",
        Install_Finish = "마침",
        FinishScreen_Title = "지퍼 리소스팩 설치 완료",
        FinishScreen_Description = "지퍼 리소스팩 설치가 완료되었습니다.\n설치 프로그램을 마치려면 '마침' 버튼을 눌러주세요.",
        FinishScreen_RunAdofai = "얼불춤 실행",
        FinishScreen_Title_Error = "지퍼 리소스팩을 설치하지 못했습니다.",
        FinishScreen_Description_Error = "지퍼 리소스팩 설치중 오류가 발생했습니다.\n자세한 오류를 확인하기 위해서는 '로그 보기' 버튼을 눌러주세요.",
        FinishScreen_CheckLog = "로그 보기"
    };

    public static Resources Current = CultureInfo.CurrentCulture.Name == "ko-KR" ? Korean : English;

    public string Title;
    public string Cancel;
    public string Previous;
    public string Next;
    public string MainScreen_Title;
    [Obsolete("", true)]
    public string MainScreen_Description;
    public string MainScreen_Description1;
    public string MainScreen_Description2;
    public string MainScreen_Description3;
    public string MainScreen_Description4;
    public string MainScreen_Description5;
    public string MainScreen_Description6;
    public string MainScreen_Description7;
    public string MainScreen_BugReport;
    public string MainScreen_Donate;
    public string MainScreen_OpenBrowserFailed;
    public string MainScreen_KakaoPayQrForm;
    public string MainScreen_NoInternet;
    public string Error;
    public string SelectLocation_Title;
    public string SelectLocation_Description;
    public string SelectLocation_Select;
    public string SelectLocation_AdofaiFolderGuide;
    public string SelectLocation_NoLocation;
    public string SelectLocation_NoFolder;
    public string SelectLocation_NoAdofai;
    public string SelectMod_Title;
    public string SelectMod_Description;
    public string SelectLocation_Location;
    public string SelectMod_Mod;
    public string Install_Install;
    public string Install_Finish;
    public string FinishScreen_Title;
    public string FinishScreen_Description;
    public string FinishScreen_RunAdofai;
    public string FinishScreen_Title_Error;
    public string FinishScreen_Description_Error;
    public string FinishScreen_CheckLog;
}
