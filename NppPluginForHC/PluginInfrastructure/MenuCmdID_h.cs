﻿// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
//
// This file should stay in sync with the CPP project file
// "notepad-plus-plus/PowerEditor/src/menuCmdID.h"
// found at
// https://github.com/notepad-plus-plus/notepad-plus-plus/blob/master/PowerEditor/src/menuCmdID.h

namespace NppPluginForHC.PluginInfrastructure
{
    public enum NppMenuCmd : uint
    {
        IDM = 40000,

        IDM_FILE = (IDM + 1000),
        IDM_FILE_NEW = (IDM_FILE + 1),
        IDM_FILE_OPEN = (IDM_FILE + 2),
        IDM_FILE_CLOSE = (IDM_FILE + 3),
        IDM_FILE_CLOSEALL = (IDM_FILE + 4),
        IDM_FILE_CLOSEALL_BUT_CURRENT = (IDM_FILE + 5),
        IDM_FILE_SAVE = (IDM_FILE + 6),
        IDM_FILE_SAVEALL = (IDM_FILE + 7),
        IDM_FILE_SAVEAS = (IDM_FILE + 8),
        //IDM_FILE_ASIAN_LANG              = (IDM_FILE + 9), 
        IDM_FILE_PRINT = (IDM_FILE + 10),
        IDM_FILE_PRINTNOW = 1001,
        IDM_FILE_EXIT = (IDM_FILE + 11),
        IDM_FILE_LOADSESSION = (IDM_FILE + 12),
        IDM_FILE_SAVESESSION = (IDM_FILE + 13),
        IDM_FILE_RELOAD = (IDM_FILE + 14),
        IDM_FILE_SAVECOPYAS = (IDM_FILE + 15),
        IDM_FILE_DELETE = (IDM_FILE + 16),
        IDM_FILE_RENAME = (IDM_FILE + 17),

        // A mettre � jour si on ajoute nouveau menu item dans le menu "File"
        IDM_FILEMENU_LASTONE = IDM_FILE_RENAME,

        IDM_EDIT = (IDM + 2000),
        IDM_EDIT_CUT = (IDM_EDIT + 1),
        IDM_EDIT_COPY = (IDM_EDIT + 2),
        IDM_EDIT_UNDO = (IDM_EDIT + 3),
        IDM_EDIT_REDO = (IDM_EDIT + 4),
        IDM_EDIT_PASTE = (IDM_EDIT + 5),
        IDM_EDIT_DELETE = (IDM_EDIT + 6),
        IDM_EDIT_SELECTALL = (IDM_EDIT + 7),

        IDM_EDIT_INS_TAB = (IDM_EDIT + 8),
        IDM_EDIT_RMV_TAB = (IDM_EDIT + 9),
        IDM_EDIT_DUP_LINE = (IDM_EDIT + 10),
        IDM_EDIT_TRANSPOSE_LINE = (IDM_EDIT + 11),
        IDM_EDIT_SPLIT_LINES = (IDM_EDIT + 12),
        IDM_EDIT_JOIN_LINES = (IDM_EDIT + 13),
        IDM_EDIT_LINE_UP = (IDM_EDIT + 14),
        IDM_EDIT_LINE_DOWN = (IDM_EDIT + 15),
        IDM_EDIT_UPPERCASE = (IDM_EDIT + 16),
        IDM_EDIT_LOWERCASE = (IDM_EDIT + 17),

        // Menu macro
        IDM_MACRO_STARTRECORDINGMACRO = (IDM_EDIT + 18),
        IDM_MACRO_STOPRECORDINGMACRO = (IDM_EDIT + 19),
        IDM_MACRO_PLAYBACKRECORDEDMACRO = (IDM_EDIT + 21),
        //-----------

        IDM_EDIT_BLOCK_COMMENT = (IDM_EDIT + 22),
        IDM_EDIT_STREAM_COMMENT = (IDM_EDIT + 23),
        IDM_EDIT_TRIMTRAILING = (IDM_EDIT + 24),
        IDM_EDIT_TRIMLINEHEAD = (IDM_EDIT + 42),
        IDM_EDIT_TRIM_BOTH = (IDM_EDIT + 43),
        IDM_EDIT_EOL2WS = (IDM_EDIT + 44),
        IDM_EDIT_TRIMALL = (IDM_EDIT + 45),
        IDM_EDIT_TAB2SW = (IDM_EDIT + 46),
        IDM_EDIT_SW2TAB = (IDM_EDIT + 47),

        // Menu macro
        IDM_MACRO_SAVECURRENTMACRO = (IDM_EDIT + 25),
        //-----------

        IDM_EDIT_RTL = (IDM_EDIT + 26),
        IDM_EDIT_LTR = (IDM_EDIT + 27),
        IDM_EDIT_SETREADONLY = (IDM_EDIT + 28),
        IDM_EDIT_FULLPATHTOCLIP = (IDM_EDIT + 29),
        IDM_EDIT_FILENAMETOCLIP = (IDM_EDIT + 30),
        IDM_EDIT_CURRENTDIRTOCLIP = (IDM_EDIT + 31),

        // Menu macro
        IDM_MACRO_RUNMULTIMACRODLG = (IDM_EDIT + 32),
        //-----------

        IDM_EDIT_CLEARREADONLY = (IDM_EDIT + 33),
        IDM_EDIT_COLUMNMODE = (IDM_EDIT + 34),
        IDM_EDIT_BLOCK_COMMENT_SET = (IDM_EDIT + 35),
        IDM_EDIT_BLOCK_UNCOMMENT = (IDM_EDIT + 36),

        IDM_EDIT_AUTOCOMPLETE = (50000 + 0),
        IDM_EDIT_AUTOCOMPLETE_CURRENTFILE = (50000 + 1),
        IDM_EDIT_FUNCCALLTIP = (50000 + 2),

        //Belong to MENU FILE
        IDM_OPEN_ALL_RECENT_FILE = (IDM_EDIT + 40),
        IDM_CLEAN_RECENT_FILE_LIST = (IDM_EDIT + 41),

        IDM_SEARCH = (IDM + 3000),

        IDM_SEARCH_FIND = (IDM_SEARCH + 1),
        IDM_SEARCH_FINDNEXT = (IDM_SEARCH + 2),
        IDM_SEARCH_REPLACE = (IDM_SEARCH + 3),
        IDM_SEARCH_GOTOLINE = (IDM_SEARCH + 4),
        IDM_SEARCH_TOGGLE_BOOKMARK = (IDM_SEARCH + 5),
        IDM_SEARCH_NEXT_BOOKMARK = (IDM_SEARCH + 6),
        IDM_SEARCH_PREV_BOOKMARK = (IDM_SEARCH + 7),
        IDM_SEARCH_CLEAR_BOOKMARKS = (IDM_SEARCH + 8),
        IDM_SEARCH_GOTOMATCHINGBRACE = (IDM_SEARCH + 9),
        IDM_SEARCH_FINDPREV = (IDM_SEARCH + 10),
        IDM_SEARCH_FINDINCREMENT = (IDM_SEARCH + 11),
        IDM_SEARCH_FINDINFILES = (IDM_SEARCH + 13),
        IDM_SEARCH_VOLATILE_FINDNEXT = (IDM_SEARCH + 14),
        IDM_SEARCH_VOLATILE_FINDPREV = (IDM_SEARCH + 15),
        IDM_SEARCH_CUTMARKEDLINES = (IDM_SEARCH + 18),
        IDM_SEARCH_COPYMARKEDLINES = (IDM_SEARCH + 19),
        IDM_SEARCH_PASTEMARKEDLINES = (IDM_SEARCH + 20),
        IDM_SEARCH_DELETEMARKEDLINES = (IDM_SEARCH + 21),
        IDM_SEARCH_MARKALLEXT1 = (IDM_SEARCH + 22),
        IDM_SEARCH_UNMARKALLEXT1 = (IDM_SEARCH + 23),
        IDM_SEARCH_MARKALLEXT2 = (IDM_SEARCH + 24),
        IDM_SEARCH_UNMARKALLEXT2 = (IDM_SEARCH + 25),
        IDM_SEARCH_MARKALLEXT3 = (IDM_SEARCH + 26),
        IDM_SEARCH_UNMARKALLEXT3 = (IDM_SEARCH + 27),
        IDM_SEARCH_MARKALLEXT4 = (IDM_SEARCH + 28),
        IDM_SEARCH_UNMARKALLEXT4 = (IDM_SEARCH + 29),
        IDM_SEARCH_MARKALLEXT5 = (IDM_SEARCH + 30),
        IDM_SEARCH_UNMARKALLEXT5 = (IDM_SEARCH + 31),
        IDM_SEARCH_CLEARALLMARKS = (IDM_SEARCH + 32),

        IDM_SEARCH_GOPREVMARKER1 = (IDM_SEARCH + 33),
        IDM_SEARCH_GOPREVMARKER2 = (IDM_SEARCH + 34),
        IDM_SEARCH_GOPREVMARKER3 = (IDM_SEARCH + 35),
        IDM_SEARCH_GOPREVMARKER4 = (IDM_SEARCH + 36),
        IDM_SEARCH_GOPREVMARKER5 = (IDM_SEARCH + 37),
        IDM_SEARCH_GOPREVMARKER_DEF = (IDM_SEARCH + 38),

        IDM_SEARCH_GONEXTMARKER1 = (IDM_SEARCH + 39),
        IDM_SEARCH_GONEXTMARKER2 = (IDM_SEARCH + 40),
        IDM_SEARCH_GONEXTMARKER3 = (IDM_SEARCH + 41),
        IDM_SEARCH_GONEXTMARKER4 = (IDM_SEARCH + 42),
        IDM_SEARCH_GONEXTMARKER5 = (IDM_SEARCH + 43),
        IDM_SEARCH_GONEXTMARKER_DEF = (IDM_SEARCH + 44),

        IDM_FOCUS_ON_FOUND_RESULTS = (IDM_SEARCH + 45),
        IDM_SEARCH_GOTONEXTFOUND = (IDM_SEARCH + 46),
        IDM_SEARCH_GOTOPREVFOUND = (IDM_SEARCH + 47),

        IDM_SEARCH_SETANDFINDNEXT = (IDM_SEARCH + 48),
        IDM_SEARCH_SETANDFINDPREV = (IDM_SEARCH + 49),
        IDM_SEARCH_INVERSEMARKS = (IDM_SEARCH + 50),

        IDM_VIEW = (IDM + 4000),
        //IDM_VIEW_TOOLBAR_HIDE            = (IDM_VIEW + 1),
        IDM_VIEW_TOOLBAR_REDUCE = (IDM_VIEW + 2),
        IDM_VIEW_TOOLBAR_ENLARGE = (IDM_VIEW + 3),
        IDM_VIEW_TOOLBAR_STANDARD = (IDM_VIEW + 4),
        IDM_VIEW_REDUCETABBAR = (IDM_VIEW + 5),
        IDM_VIEW_LOCKTABBAR = (IDM_VIEW + 6),
        IDM_VIEW_DRAWTABBAR_TOPBAR = (IDM_VIEW + 7),
        IDM_VIEW_DRAWTABBAR_INACIVETAB = (IDM_VIEW + 8),
        IDM_VIEW_POSTIT = (IDM_VIEW + 9),
        IDM_VIEW_TOGGLE_FOLDALL = (IDM_VIEW + 10),
        IDM_VIEW_USER_DLG = (IDM_VIEW + 11),
        IDM_VIEW_LINENUMBER = (IDM_VIEW + 12),
        IDM_VIEW_SYMBOLMARGIN = (IDM_VIEW + 13),
        IDM_VIEW_FOLDERMAGIN = (IDM_VIEW + 14),
        IDM_VIEW_FOLDERMAGIN_SIMPLE = (IDM_VIEW + 15),
        IDM_VIEW_FOLDERMAGIN_ARROW = (IDM_VIEW + 16),
        IDM_VIEW_FOLDERMAGIN_CIRCLE = (IDM_VIEW + 17),
        IDM_VIEW_FOLDERMAGIN_BOX = (IDM_VIEW + 18),
        IDM_VIEW_ALL_CHARACTERS = (IDM_VIEW + 19),
        IDM_VIEW_INDENT_GUIDE = (IDM_VIEW + 20),
        IDM_VIEW_CURLINE_HILITING = (IDM_VIEW + 21),
        IDM_VIEW_WRAP = (IDM_VIEW + 22),
        IDM_VIEW_ZOOMIN = (IDM_VIEW + 23),
        IDM_VIEW_ZOOMOUT = (IDM_VIEW + 24),
        IDM_VIEW_TAB_SPACE = (IDM_VIEW + 25),
        IDM_VIEW_EOL = (IDM_VIEW + 26),
        IDM_VIEW_EDGELINE = (IDM_VIEW + 27),
        IDM_VIEW_EDGEBACKGROUND = (IDM_VIEW + 28),
        IDM_VIEW_TOGGLE_UNFOLDALL = (IDM_VIEW + 29),
        IDM_VIEW_FOLD_CURRENT = (IDM_VIEW + 30),
        IDM_VIEW_UNFOLD_CURRENT = (IDM_VIEW + 31),
        IDM_VIEW_FULLSCREENTOGGLE = (IDM_VIEW + 32),
        IDM_VIEW_ZOOMRESTORE = (IDM_VIEW + 33),
        IDM_VIEW_ALWAYSONTOP = (IDM_VIEW + 34),
        IDM_VIEW_SYNSCROLLV = (IDM_VIEW + 35),
        IDM_VIEW_SYNSCROLLH = (IDM_VIEW + 36),
        IDM_VIEW_EDGENONE = (IDM_VIEW + 37),
        IDM_VIEW_DRAWTABBAR_CLOSEBOTTUN = (IDM_VIEW + 38),
        IDM_VIEW_DRAWTABBAR_DBCLK2CLOSE = (IDM_VIEW + 39),
        IDM_VIEW_REFRESHTABAR = (IDM_VIEW + 40),
        IDM_VIEW_WRAP_SYMBOL = (IDM_VIEW + 41),
        IDM_VIEW_HIDELINES = (IDM_VIEW + 42),
        IDM_VIEW_DRAWTABBAR_VERTICAL = (IDM_VIEW + 43),
        IDM_VIEW_DRAWTABBAR_MULTILINE = (IDM_VIEW + 44),
        IDM_VIEW_DOCCHANGEMARGIN = (IDM_VIEW + 45),
        IDM_VIEW_LWDEF = (IDM_VIEW + 46),
        IDM_VIEW_LWALIGN = (IDM_VIEW + 47),
        IDM_VIEW_LWINDENT = (IDM_VIEW + 48),
        IDM_VIEW_SUMMARY = (IDM_VIEW + 49),

        IDM_VIEW_FOLD = (IDM_VIEW + 50),
        IDM_VIEW_FOLD_1 = (IDM_VIEW_FOLD + 1),
        IDM_VIEW_FOLD_2 = (IDM_VIEW_FOLD + 2),
        IDM_VIEW_FOLD_3 = (IDM_VIEW_FOLD + 3),
        IDM_VIEW_FOLD_4 = (IDM_VIEW_FOLD + 4),
        IDM_VIEW_FOLD_5 = (IDM_VIEW_FOLD + 5),
        IDM_VIEW_FOLD_6 = (IDM_VIEW_FOLD + 6),
        IDM_VIEW_FOLD_7 = (IDM_VIEW_FOLD + 7),
        IDM_VIEW_FOLD_8 = (IDM_VIEW_FOLD + 8),

        IDM_VIEW_UNFOLD = (IDM_VIEW + 60),
        IDM_VIEW_UNFOLD_1 = (IDM_VIEW_UNFOLD + 1),
        IDM_VIEW_UNFOLD_2 = (IDM_VIEW_UNFOLD + 2),
        IDM_VIEW_UNFOLD_3 = (IDM_VIEW_UNFOLD + 3),
        IDM_VIEW_UNFOLD_4 = (IDM_VIEW_UNFOLD + 4),
        IDM_VIEW_UNFOLD_5 = (IDM_VIEW_UNFOLD + 5),
        IDM_VIEW_UNFOLD_6 = (IDM_VIEW_UNFOLD + 6),
        IDM_VIEW_UNFOLD_7 = (IDM_VIEW_UNFOLD + 7),
        IDM_VIEW_UNFOLD_8 = (IDM_VIEW_UNFOLD + 8),

        IDM_VIEW_GOTO_ANOTHER_VIEW = 10001,
        IDM_VIEW_CLONE_TO_ANOTHER_VIEW = 10002,
        IDM_VIEW_GOTO_NEW_INSTANCE = 10003,
        IDM_VIEW_LOAD_IN_NEW_INSTANCE = 10004,

        IDM_VIEW_SWITCHTO_OTHER_VIEW = (IDM_VIEW + 72),

        IDM_FORMAT = (IDM + 5000),
        IDM_FORMAT_TODOS = (IDM_FORMAT + 1),
        IDM_FORMAT_TOUNIX = (IDM_FORMAT + 2),
        IDM_FORMAT_TOMAC = (IDM_FORMAT + 3),
        IDM_FORMAT_ANSI = (IDM_FORMAT + 4),
        IDM_FORMAT_UTF_8 = (IDM_FORMAT + 5),
        IDM_FORMAT_UCS_2BE = (IDM_FORMAT + 6),
        IDM_FORMAT_UCS_2LE = (IDM_FORMAT + 7),
        IDM_FORMAT_AS_UTF_8 = (IDM_FORMAT + 8),
        IDM_FORMAT_CONV2_ANSI = (IDM_FORMAT + 9),
        IDM_FORMAT_CONV2_AS_UTF_8 = (IDM_FORMAT + 10),
        IDM_FORMAT_CONV2_UTF_8 = (IDM_FORMAT + 11),
        IDM_FORMAT_CONV2_UCS_2BE = (IDM_FORMAT + 12),
        IDM_FORMAT_CONV2_UCS_2LE = (IDM_FORMAT + 13),

        IDM_FORMAT_ENCODE = (IDM_FORMAT + 20),
        IDM_FORMAT_WIN_1250 = (IDM_FORMAT_ENCODE + 0),
        IDM_FORMAT_WIN_1251 = (IDM_FORMAT_ENCODE + 1),
        IDM_FORMAT_WIN_1252 = (IDM_FORMAT_ENCODE + 2),
        IDM_FORMAT_WIN_1253 = (IDM_FORMAT_ENCODE + 3),
        IDM_FORMAT_WIN_1254 = (IDM_FORMAT_ENCODE + 4),
        IDM_FORMAT_WIN_1255 = (IDM_FORMAT_ENCODE + 5),
        IDM_FORMAT_WIN_1256 = (IDM_FORMAT_ENCODE + 6),
        IDM_FORMAT_WIN_1257 = (IDM_FORMAT_ENCODE + 7),
        IDM_FORMAT_WIN_1258 = (IDM_FORMAT_ENCODE + 8),
        IDM_FORMAT_ISO_8859_1 = (IDM_FORMAT_ENCODE + 9),
        IDM_FORMAT_ISO_8859_2 = (IDM_FORMAT_ENCODE + 10),
        IDM_FORMAT_ISO_8859_3 = (IDM_FORMAT_ENCODE + 11),
        IDM_FORMAT_ISO_8859_4 = (IDM_FORMAT_ENCODE + 12),
        IDM_FORMAT_ISO_8859_5 = (IDM_FORMAT_ENCODE + 13),
        IDM_FORMAT_ISO_8859_6 = (IDM_FORMAT_ENCODE + 14),
        IDM_FORMAT_ISO_8859_7 = (IDM_FORMAT_ENCODE + 15),
        IDM_FORMAT_ISO_8859_8 = (IDM_FORMAT_ENCODE + 16),
        IDM_FORMAT_ISO_8859_9 = (IDM_FORMAT_ENCODE + 17),
        IDM_FORMAT_ISO_8859_10 = (IDM_FORMAT_ENCODE + 18),
        IDM_FORMAT_ISO_8859_11 = (IDM_FORMAT_ENCODE + 19),
        IDM_FORMAT_ISO_8859_13 = (IDM_FORMAT_ENCODE + 20),
        IDM_FORMAT_ISO_8859_14 = (IDM_FORMAT_ENCODE + 21),
        IDM_FORMAT_ISO_8859_15 = (IDM_FORMAT_ENCODE + 22),
        IDM_FORMAT_ISO_8859_16 = (IDM_FORMAT_ENCODE + 23),
        IDM_FORMAT_DOS_437 = (IDM_FORMAT_ENCODE + 24),
        IDM_FORMAT_DOS_720 = (IDM_FORMAT_ENCODE + 25),
        IDM_FORMAT_DOS_737 = (IDM_FORMAT_ENCODE + 26),
        IDM_FORMAT_DOS_775 = (IDM_FORMAT_ENCODE + 27),
        IDM_FORMAT_DOS_850 = (IDM_FORMAT_ENCODE + 28),
        IDM_FORMAT_DOS_852 = (IDM_FORMAT_ENCODE + 29),
        IDM_FORMAT_DOS_855 = (IDM_FORMAT_ENCODE + 30),
        IDM_FORMAT_DOS_857 = (IDM_FORMAT_ENCODE + 31),
        IDM_FORMAT_DOS_858 = (IDM_FORMAT_ENCODE + 32),
        IDM_FORMAT_DOS_860 = (IDM_FORMAT_ENCODE + 33),
        IDM_FORMAT_DOS_861 = (IDM_FORMAT_ENCODE + 34),
        IDM_FORMAT_DOS_862 = (IDM_FORMAT_ENCODE + 35),
        IDM_FORMAT_DOS_863 = (IDM_FORMAT_ENCODE + 36),
        IDM_FORMAT_DOS_865 = (IDM_FORMAT_ENCODE + 37),
        IDM_FORMAT_DOS_866 = (IDM_FORMAT_ENCODE + 38),
        IDM_FORMAT_DOS_869 = (IDM_FORMAT_ENCODE + 39),
        IDM_FORMAT_BIG5 = (IDM_FORMAT_ENCODE + 40),
        IDM_FORMAT_GB2312 = (IDM_FORMAT_ENCODE + 41),
        IDM_FORMAT_SHIFT_JIS = (IDM_FORMAT_ENCODE + 42),
        IDM_FORMAT_KOREAN_WIN = (IDM_FORMAT_ENCODE + 43),
        IDM_FORMAT_EUC_KR = (IDM_FORMAT_ENCODE + 44),
        IDM_FORMAT_TIS_620 = (IDM_FORMAT_ENCODE + 45),
        IDM_FORMAT_MAC_CYRILLIC = (IDM_FORMAT_ENCODE + 46),
        IDM_FORMAT_KOI8U_CYRILLIC = (IDM_FORMAT_ENCODE + 47),
        IDM_FORMAT_KOI8R_CYRILLIC = (IDM_FORMAT_ENCODE + 48),
        IDM_FORMAT_ENCODE_END = IDM_FORMAT_KOI8R_CYRILLIC,

        //#define    IDM_FORMAT_CONVERT            200

        IDM_LANG = (IDM + 6000),
        IDM_LANGSTYLE_CONFIG_DLG = (IDM_LANG + 1),
        IDM_LANG_C = (IDM_LANG + 2),
        IDM_LANG_CPP = (IDM_LANG + 3),
        IDM_LANG_JAVA = (IDM_LANG + 4),
        IDM_LANG_HTML = (IDM_LANG + 5),
        IDM_LANG_XML = (IDM_LANG + 6),
        IDM_LANG_JS = (IDM_LANG + 7),
        IDM_LANG_PHP = (IDM_LANG + 8),
        IDM_LANG_ASP = (IDM_LANG + 9),
        IDM_LANG_CSS = (IDM_LANG + 10),
        IDM_LANG_PASCAL = (IDM_LANG + 11),
        IDM_LANG_PYTHON = (IDM_LANG + 12),
        IDM_LANG_PERL = (IDM_LANG + 13),
        IDM_LANG_OBJC = (IDM_LANG + 14),
        IDM_LANG_ASCII = (IDM_LANG + 15),
        IDM_LANG_TEXT = (IDM_LANG + 16),
        IDM_LANG_RC = (IDM_LANG + 17),
        IDM_LANG_MAKEFILE = (IDM_LANG + 18),
        IDM_LANG_INI = (IDM_LANG + 19),
        IDM_LANG_SQL = (IDM_LANG + 20),
        IDM_LANG_VB = (IDM_LANG + 21),
        IDM_LANG_BATCH = (IDM_LANG + 22),
        IDM_LANG_CS = (IDM_LANG + 23),
        IDM_LANG_LUA = (IDM_LANG + 24),
        IDM_LANG_TEX = (IDM_LANG + 25),
        IDM_LANG_FORTRAN = (IDM_LANG + 26),
        IDM_LANG_BASH = (IDM_LANG + 27),
        IDM_LANG_FLASH = (IDM_LANG + 28),
        IDM_LANG_NSIS = (IDM_LANG + 29),
        IDM_LANG_TCL = (IDM_LANG + 30),
        IDM_LANG_LISP = (IDM_LANG + 31),
        IDM_LANG_SCHEME = (IDM_LANG + 32),
        IDM_LANG_ASM = (IDM_LANG + 33),
        IDM_LANG_DIFF = (IDM_LANG + 34),
        IDM_LANG_PROPS = (IDM_LANG + 35),
        IDM_LANG_PS = (IDM_LANG + 36),
        IDM_LANG_RUBY = (IDM_LANG + 37),
        IDM_LANG_SMALLTALK = (IDM_LANG + 38),
        IDM_LANG_VHDL = (IDM_LANG + 39),
        IDM_LANG_CAML = (IDM_LANG + 40),
        IDM_LANG_KIX = (IDM_LANG + 41),
        IDM_LANG_ADA = (IDM_LANG + 42),
        IDM_LANG_VERILOG = (IDM_LANG + 43),
        IDM_LANG_AU3 = (IDM_LANG + 44),
        IDM_LANG_MATLAB = (IDM_LANG + 45),
        IDM_LANG_HASKELL = (IDM_LANG + 46),
        IDM_LANG_INNO = (IDM_LANG + 47),
        IDM_LANG_CMAKE = (IDM_LANG + 48),
        IDM_LANG_YAML = (IDM_LANG + 49),
        IDM_LANG_COBOL = (IDM_LANG + 50),
        IDM_LANG_D = (IDM_LANG + 51),
        IDM_LANG_GUI4CLI = (IDM_LANG + 52),
        IDM_LANG_POWERSHELL = (IDM_LANG + 53),
        IDM_LANG_R = (IDM_LANG + 54),
        IDM_LANG_JSP = (IDM_LANG + 55),
        IDM_LANG_EXTERNAL = (IDM_LANG + 65),
        IDM_LANG_EXTERNAL_LIMIT = (IDM_LANG + 79),
        IDM_LANG_USER = (IDM_LANG + 80),     //46080
        IDM_LANG_USER_LIMIT = (IDM_LANG + 110),    //46110

        IDM_ABOUT = (IDM + 7000),
        IDM_HOMESWEETHOME = (IDM_ABOUT + 1),
        IDM_PROJECTPAGE = (IDM_ABOUT + 2),
        IDM_ONLINEHELP = (IDM_ABOUT + 3),
        IDM_FORUM = (IDM_ABOUT + 4),
        IDM_PLUGINSHOME = (IDM_ABOUT + 5),
        IDM_UPDATE_NPP = (IDM_ABOUT + 6),
        IDM_WIKIFAQ = (IDM_ABOUT + 7),
        IDM_HELP = (IDM_ABOUT + 8),

        IDM_SETTING = (IDM + 8000),
        IDM_SETTING_TAB_SIZE = (IDM_SETTING + 1),
        IDM_SETTING_TAB_REPLCESPACE = (IDM_SETTING + 2),
        IDM_SETTING_HISTORY_SIZE = (IDM_SETTING + 3),
        IDM_SETTING_EDGE_SIZE = (IDM_SETTING + 4),
        IDM_SETTING_IMPORTPLUGIN = (IDM_SETTING + 5),
        IDM_SETTING_IMPORTSTYLETHEMS = (IDM_SETTING + 6),
        IDM_SETTING_TRAYICON = (IDM_SETTING + 8),
        IDM_SETTING_SHORTCUT_MAPPER = (IDM_SETTING + 9),
        IDM_SETTING_REMEMBER_LAST_SESSION = (IDM_SETTING + 10),
        IDM_SETTING_PREFERECE = (IDM_SETTING + 11),
        IDM_SETTING_AUTOCNBCHAR = (IDM_SETTING + 15),
        IDM_SETTING_SHORTCUT_MAPPER_MACRO = (IDM_SETTING + 16),
        IDM_SETTING_SHORTCUT_MAPPER_RUN = (IDM_SETTING + 17),
        IDM_SETTING_EDITCONTEXTMENU = (IDM_SETTING + 18),

        IDM_EXECUTE = (IDM + 9000),

        IDM_SYSTRAYPOPUP = (IDM + 3100),
        IDM_SYSTRAYPOPUP_ACTIVATE = (IDM_SYSTRAYPOPUP + 1),
        IDM_SYSTRAYPOPUP_NEWDOC = (IDM_SYSTRAYPOPUP + 2),
        IDM_SYSTRAYPOPUP_NEW_AND_PASTE = (IDM_SYSTRAYPOPUP + 3),
        IDM_SYSTRAYPOPUP_OPENFILE = (IDM_SYSTRAYPOPUP + 4),
        IDM_SYSTRAYPOPUP_CLOSE = (IDM_SYSTRAYPOPUP + 5)
    }
}
