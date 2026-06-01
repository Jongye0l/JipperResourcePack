using System;
using System.Globalization;

namespace JipperResourcePack.Installer.Resource;

public class Resources {
    public static readonly Resources English = new() {
        Title = "Jipper ResourcePack Installer",
        Cancel = "Cancel",
        Previous = "< Previous",
        Next = "Next >",
        MainScreen_Title = "Starting the installation of the Jipper Resource Pack.",
        MainScreen_Description1 = "Installs the Jipper Resource Pack to A Dance of Fire and Ice.",
        MainScreen_Description2 = "Internet access to Github is required for installation.",
        MainScreen_Description3 = "Required system(UMM, JALib) may be automatically installed.",
        MainScreen_Description4 = "The Jipper Resource Pack only supports the latest older, default, beta, and alpha branches. It may not work on other versions.",
        MainScreen_Description5 = "Proceeding with the installation will force close A Dance of Fire and Ice.",
        MainScreen_Description6 = "This program is a community mod, not an official program by 7th Beat Games.",
        MainScreen_Description7 = "Click the 'Next' button to continue.",
        MainScreen_BugReport = "Bug Report / Inquiry",
        MainScreen_Donate = "Donate",
        MainScreen_KakaoPayQrForm = "Jipper Resource Pack Donation KakaoPay QR",
        MainScreen_OpenBrowserFailed = "Failed to open the browser. The URL has been copied to the clipboard.",
        MainScreen_NoInternet = "Not connected to the internet.",
        Warn = "Warning",
        Error = "Error",
        Title1 = "Select Folder",
        Title2 = "Select Components",
        Title3 = "Install",
        Title4 = "Finish",
        SelectLocation_Title = "Please select the A Dance of Fire and Ice folder.",
        SelectLocation_Description = "Install the Jipper ResourcePack in the Adofai.\nIf you install it in a different Adofai folder or if the program has not found the Adofai folder,\nclick the 'Select Folder' button to select the Adofai folder.",
        SelectLocation_Select = "Browse",
        SelectLocation_Location = "Select ADOFAI Folder",
        // SelectLocation_AdofaiFolderGuide = "A Dance of Fire and Ice Folder Guide",
        SelectLocation_NoLocation = "Location has not been set.",
        SelectLocation_NoFolder = "Folder does not exist.",
        SelectLocation_NoAdofai = "The selected folder is not an A Dance of Fire and Ice folder.",
        SelectLocation_NotifyTitle = "Path Analysis",
        SelectLocation_NotifyEmpty = "Folder location is not set",
        SelectLocation_NotifyFolderNotExist = "Folder does not exist",
        SelectLocation_NotifyFolderExist = "Folder exists",
        SelectLocation_NotifyGameNotFound = "Game executable or data could not be found",
        SelectLocation_NotifyGameFound = "Game found",
        SelectLocation_NotifyUmmIsAssembly = "UMM is installed in Assembly format",
        SelectLocation_NotifyUmmIsAssembly2 = "It must be reinstalled every time A Dance of Fire and Ice is updated.",
        SelectLocation_NotifyDoorstopIsOld = "Doorstop is an outdated version.",
        SelectLocation_NotifyDoorstopIsOld2 = "Crashes will occur from r141 onwards, making it unusable.",
        Select_Title = "Please select the components to install.",
        Select_Work = "Task",
        Select_Work_Install = "Install",
        Select_Work_Uninstall = "Uninstall",
        Select_Requirement = "Install Required Components",
        Select_Requirement_UnityModManager = "Unity Mod Manager is required to use the Jipper Resource Pack.\nIt cannot be excluded because Unity Mod Manager is not installed.",
        Select_Requirement_JALib = "JALib is required to use the Jipper Resource Pack.\nIt cannot be excluded because JALib is not installed.",
        Select_Requirement_Doorstop_Umm = "Doorstop is required to use Unity Mod Manager.\nIt cannot be excluded since Unity Mod Manager is selected.",
        Select_Requirement_Doorstop_Asm = "Unity Mod Manager is currently installed via the Assembly method.\nThe Assembly method requires a reinstallation every time the game updates. Do you wish to continue?",
        Select_Requirement_Doorstop_Old = "The Doorstop version currently in use is outdated.\nFrom r141 onwards, crashes will occur if Doorstop is not version 5.4.0 or higher. Do you wish to continue?",
        Select_Requirement_JipperResourcePack = "The Jipper Resource Pack cannot be excluded because it is not installed.",
        Select_AdditionMods = "Install Additional Mods",
        Select_RemoveMods = "Remove Existing Mods",
        Select_RemoveMods_Confirm = "Are you sure you want to remove the selected mods?\nRemoved mods cannot be recovered, and data may not be preserved even if reinstalled.",
        Select_UninstallOption = "Uninstall Options",
        Select_UninstallOption_All = "Full Uninstall",
        Select_UninstallOption_AllDescription = "Completely removes the mod manager and all mods. You will need to reinstall all mods to use them again.",
        Select_UninstallOption_OnlyManager = "Remove Mod Manager Only",
        Select_UninstallOption_OnlyManagerDescription = "Removes only the mod manager while keeping the mod data. You can use the mods again just by reinstalling the mod manager.",
        Select_UninstallOption_OnlyMod = "Remove Specific Mods Only",
        Select_UninstallOption_OnlyModDescription = "Removes only specific mods instead of erasing all of them. The remaining mods will continue to work.",
        FinishScreen_Finish = "Exit",
        FinishScreen_Title_Install = "Installation Complete",
        FinishScreen_Title_Uninstall = "Uninstallation Complete",
        FinishScreen_Description_Install = "The Jipper ResourcePack installation is complete.\nClick the 'Exit' button to close the program.",
        FinishScreen_Description_Uninstall = "The Jipper Resource Pack has been successfully removed.\nClick the 'Exit' button to close the program.",
        FinishScreen_RunAdofai = "Launch A Dance of Fire and Ice",
        FinishScreen_Title_Error = "Failed to install the Jipper Resource Pack.",
        FinishScreen_Description_Error = "An error occurred during the Jipper Resource Pack installation.\nClick the 'View Log' button to check the error details.",
        FinishScreen_CheckLog = "View Log"
    };

    public static readonly Resources Korean = new() {
        Title = "지퍼리소스팩 딸깍 설치기",
        Cancel = "취소",
        Previous = "< 이전",
        Next = "다음 >",
        MainScreen_Title = "지퍼 리소스팩 설치를 시작합니다.",
        MainScreen_Description1 = "불과 얼음의 춤에 지퍼 리소스팩을 설치합니다.",
        MainScreen_Description2 = "설치를 위해 Github에 접속할 수 있는 환경이 필요합니다.",
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
        Warn = "경고",
        Error = "오류",
        Title1 = "폴더 선택",
        Title2 = "설치 요소 선택",
        Title3 = "설치",
        Title4 = "완료",
        SelectLocation_Title = "얼불춤의 폴더를 선택해주세요.",
        SelectLocation_Description = "지퍼 리소스팩을 얼불춤에 설치합니다.\n다른 얼불춤 폴더에 설치하거나 얼불춤 폴더를 프로그램이 찾지 못했다면 '찾아보기' 버튼을 눌러 얼불춤 폴더를 선택해주세요.",
        SelectLocation_Select = "찾아보기",
        SelectLocation_Location = "얼불춤 폴더 선택",
        // SelectLocation_AdofaiFolderGuide = "얼불춤 폴더 가이드",
        SelectLocation_NoLocation = "위치가 설정되지 않았습니다.",
        SelectLocation_NoFolder = "폴더가 존재하지 않습니다.",
        SelectLocation_NoAdofai = "선택된 폴더는 얼불춤 폴더가 아닙니다.",
        SelectLocation_NotifyTitle = "경로 분석",
        SelectLocation_NotifyEmpty = "폴더 위치가 설정되있지 않습니다",
        SelectLocation_NotifyFolderNotExist = "폴더가 존재하지 않습니다",
        SelectLocation_NotifyFolderExist = "폴더가 존재합니다",
        SelectLocation_NotifyGameNotFound = "게임 프로그램이나 데이터를 찾을 수 없습니다",
        SelectLocation_NotifyGameFound = "게임을 찾았습니다",
        SelectLocation_NotifyUmmIsAssembly = "UMM이 Assembly 형식으로 설치되었습니다",
        SelectLocation_NotifyUmmIsAssembly2 = "얼불춤을 업데이트 할 때 마다 재설치 해야 합니다.",
        SelectLocation_NotifyDoorstopIsOld = "Doorstop이 구버전입니다.",
        SelectLocation_NotifyDoorstopIsOld2 = "r141부터 크래시가 발생하며 사용할 수 없습니다.",
        Select_Title = "설치 요소를 선택해주세요.",
        Select_Work = "작업",
        Select_Work_Install = "설치",
        Select_Work_Uninstall = "제거",
        Select_Requirement = "필수 요소 설치",
        Select_Requirement_UnityModManager = "지퍼 리소스팩을 사용하기 위해서는 유니티 모드 매니저가 필요합니다.\n유니티 모드 매니저가 설치되어 있지 않아 제외할 수 없습니다.",
        Select_Requirement_JALib = "지퍼 리소스팩을 사용하기 위해서는 JALib이 필요합니다.\nJALib이 설치되어 있지 않아 제외할 수 없습니다.",
        Select_Requirement_Doorstop_Umm = "유니티 모드 매니저를 사용하기 위해서는 Doorstop이 필요합니다.\n유니티 모드 매니저가 선택되어 제외할 수 없습니다.",
        Select_Requirement_Doorstop_Asm = "현재 유니티 모드 매니저가 Assembly 방식으로 설치되어 있습니다.\nAssembly 방식은 게임을 업데이트 할 때 마다 새로 설치해야 합니다. 계속 하시겠습니까?",
        Select_Requirement_Doorstop_Old = "현재 사용중인 Doorstop이 구버전 입니다.\nr141부터는 5.4.0이상에 Doorstop이 아닐 경우 크래시가 발생합니다. 계속 하시겠습니까?",
        Select_Requirement_JipperResourcePack = "지퍼 리소스팩이 설치되어 있지 않아 제외할 수 없습니다.",
        Select_AdditionMods = "추가 모드 설치",
        Select_RemoveMods = "기존 모드 제거",
        Select_RemoveMods_Confirm = "선택한 모드를 정말 제거하시겠습니까?\n제거된 모드는 복구할 수 없으며 다시 설치해도 데이터가 유지되지 않을 수 있습니다.",
        Select_UninstallOption = "제거 옵션",
        Select_UninstallOption_All = "전체 제거",
        Select_UninstallOption_AllDescription = "모드 매니저와 모든 모드를 완전히 제거합니다. 모드를 다시 사용하기 위해서는 모든 모드를 다시 설치해야 합니다.",
        Select_UninstallOption_OnlyManager = "모드 매니저만 제거",
        Select_UninstallOption_OnlyManagerDescription = "모드 데이터는 남겨둔 채로 모드 매니저만 제거합니다. 모드 매니저를 다시 설치하면 그대로 모드를 사용할 수 있습니다.",
        Select_UninstallOption_OnlyMod = "특정 모드만 제거",
        Select_UninstallOption_OnlyModDescription = "모든 모드를 지우지 않고 특정 모드만 제거합니다. 나머지 모드는 계속해서 사용할 수 있습니다.",
        FinishScreen_Finish = "마침",
        FinishScreen_Title_Install = "지퍼 리소스팩 설치 완료",
        FinishScreen_Title_Uninstall = "지퍼 리소스팩 제거 완료",
        FinishScreen_Description_Install = "지퍼 리소스팩 설치가 완료되었습니다.\n프로그램을 마치려면 '마침' 버튼을 눌러주세요.",
        FinishScreen_Description_Uninstall = "지퍼 리소스팩 제거가 완료되었습니다.\n프로그램을 마치려면 '마침' 버튼을 눌러주세요.",
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
    public string Warn;
    public string Error;
    public string Title1;
    public string Title2;
    public string Title3;
    public string Title4;
    public string SelectLocation_Title;
    public string SelectLocation_Description;
    public string SelectLocation_Select;
    public string SelectLocation_Location;
    // public string SelectLocation_AdofaiFolderGuide;
    public string SelectLocation_NoLocation;
    public string SelectLocation_NoFolder;
    public string SelectLocation_NoAdofai;
    public string SelectLocation_NotifyTitle;
    public string SelectLocation_NotifyEmpty;
    public string SelectLocation_NotifyFolderNotExist;
    public string SelectLocation_NotifyFolderExist;
    public string SelectLocation_NotifyGameNotFound;
    public string SelectLocation_NotifyGameFound;
    public string SelectLocation_NotifyUmmIsAssembly;
    public string SelectLocation_NotifyUmmIsAssembly2;
    public string SelectLocation_NotifyDoorstopIsOld;
    public string SelectLocation_NotifyDoorstopIsOld2;
    public string Select_Title;
    public string Select_Work;
    public string Select_Work_Install;
    public string Select_Work_Uninstall;
    public string Select_Requirement;
    public string Select_Requirement_UnityModManager;
    public string Select_Requirement_JALib;
    public string Select_Requirement_Doorstop_Umm;
    public string Select_Requirement_Doorstop_Asm;
    public string Select_Requirement_Doorstop_Old;
    public string Select_Requirement_JipperResourcePack;
    public string Select_AdditionMods;
    public string Select_RemoveMods;
    public string Select_RemoveMods_Confirm;
    public string Select_UninstallOption;
    public string Select_UninstallOption_All;
    public string Select_UninstallOption_AllDescription;
    public string Select_UninstallOption_OnlyManager;
    public string Select_UninstallOption_OnlyManagerDescription;
    public string Select_UninstallOption_OnlyMod;
    public string Select_UninstallOption_OnlyModDescription;
    public string FinishScreen_Finish;
    public string FinishScreen_Title_Install;
    public string FinishScreen_Title_Uninstall;
    public string FinishScreen_Description_Install;
    public string FinishScreen_Description_Uninstall;
    public string FinishScreen_RunAdofai;
    public string FinishScreen_Title_Error;
    public string FinishScreen_Description_Error;
    public string FinishScreen_CheckLog;
}
