/*!
 * @license Copyright (c) 2003-2015, CKSource - Frederico Knabben. All rights reserved.
 * This software is covered by CKEditor Commercial License. Usage without proper license is prohibited.
 */

CKEDITOR.plugins.setLang( 'a11yhelp', 'ja', {
  "title": "ユーザー補助の説明",
  "contents": "ヘルプ コンテンツ。このダイアログを閉じるには、Esc キーを押してください。",
  "legend": [
    {
      "name": "全般",
      "items": [
        {
          "name": "エディターのツール バー",
          "legend": "${toolbarFocus} を押すと、ツール バーに移動します。Tab キーまたは Shift + Tab キーで次または前のツール バー グループに移動します。右方向キーまたは左方向キーで次または前のツール バー ボタンに移動します。Space キーまたは Enter キーを押すと、ツール バー ボタンをアクティブ化できます。"
        },
        {
          "name": "エディターのダイアログ",
          "legend": "ダイアログ内では、Tab キーを押すと、次のダイアログ要素に移動します。Shift + Tab キーを押すと、前のダイアログ要素に移動します。Enter キーを押すと、ダイアログを送信します。Esc キーを押すと、ダイアログをキャンセルします。ダイアログに複数のタブがある場合は、Alt + F10 キーを押すか、Tab キーを押してダイアログのタブ順序に従って、タブ リストに移動できます。タブ リストにフォーカスがある場合は、右方向キーと左方向キーでそれぞれ次と前のタブに移動できます。"
        },
        {
          "name": "エディターのコンテキスト メニュー",
          "legend": "${contextMenu} またはアプリケーション キーを押すと、コンテキスト メニューが開きます。Tab キーまたは下方向キーで次のメニュー オプションに移動します。Shift + Tab キーまたは上方向キーで前のオプションに移動します。Space キーまたは Enter キーを押すと、メニュー オプションを選択できます。Space キー、Enter キー、または右方向キーで現在のオプションのサブメニューが開きます。Esc キーまたは左方向キーで親メニュー項目に戻ります。Esc キーでコンテキスト メニューをキャンセルします。"
        },
        {
          "name": "エディターのリスト ボックス",
          "legend": "リスト ボックス内では、Tab キーまたは下方向キーで次のリスト項目に移動します。Shift + Tab キーまたは上方向キーで前のリスト項目に移動します。Space キーまたは Enter キーを押すと、リスト オプションを選択できます。Esc キーを押すと、リスト ボックスが閉じます。"
        },
        {
          "name": "エディターの要素パス バー",
          "legend": "${elementsPathFocus} を押すと、要素パス バーに移動します。Tab キーまたは右方向キーで次の要素ボタンに移動します。Shift + Tab キーまたは左方向キーで前の要素ボタンに移動します。Space キーまたは Enter キーを押すと、エディター内の要素を選択できます。"
        }
      ]
    },
    {
      "name": "コマンド",
      "items": [
        {
          "name": " 元に戻すコマンド",
          "legend": "${undo} を押す"
        },
        {
          "name": " やり直しコマンド",
          "legend": "${redo} を押す"
        },
        {
          "name": " 太字コマンド",
          "legend": "${bold} を押す"
        },
        {
          "name": " 斜体コマンド",
          "legend": "${italic} を押す"
        },
        {
          "name": " 下線コマンド",
          "legend": "${underline} を押す"
        },
        {
          "name": " リンク コマンド",
          "legend": "${link} を押す"
        },
        {
          "name": " ツール バーの非表示コマンド",
          "legend": "${toolbarCollapse} を押す"
        },
        {
          "name": " 前のフォーカス スペースにアクセスするコマンド",
          "legend": "${accessPreviousSpace} を押すと、カーソルより前にある方向キーで入り込むことができない一番近くのフォーカス スペースに移動できます。たとえば、HR 要素が 2 つ接している場合などです。離れたフォーカス スペースに移動するには、複数回キーを押します。"
        },
        {
          "name": " 次のフォーカス スペースにアクセスするコマンド",
          "legend": "${accessNextSpace} を押すと、カーソルより後にある方向キーで入り込むことができない一番近くのフォーカス スペースに移動できます。たとえば、HR 要素が 2 つ接している場合などです。離れたフォーカス スペースに移動するには、複数回キーを押します。"
        },
        {
          "name": " ユーザー補助のヘルプ",
          "legend": "${a11yHelp} を押す"
        }
      ]
    }
  ],
  "backspace": "BackSpace",
  "tab": "Tab",
  "enter": "Enter",
  "shift": "Shift",
  "ctrl": "Ctrl",
  "alt": "Alt",
  "pause": "Pause",
  "capslock": "CapsLock",
  "escape": "Esc",
  "pageUp": "PageUp",
  "pageDown": "PageDown",
  "end": "End",
  "home": "Home",
  "leftArrow": "←",
  "upArrow": "↑",
  "rightArrow": "→",
  "downArrow": "↓",
  "insert": "Ins",
  "delete": "Del",
  "leftWindowKey": "左 Windows キー",
  "rightWindowKey": "右 Windows キー",
  "selectKey": "キーの選択",
  "numpad0": "テンキー 0",
  "numpad1": "テンキー 1",
  "numpad2": "テンキー 2",
  "numpad3": "テンキー 3",
  "numpad4": "テンキー 4",
  "numpad5": "テンキー 5",
  "numpad6": "テンキー 6",
  "numpad7": "テンキー 7",
  "numpad8": "テンキー 8",
  "numpad9": "テンキー 9",
  "multiply": "乗算",
  "add": "加算",
  "subtract": "減算",
  "decimalPoint": "小数点",
  "divide": "除算",
  "f1": "F1",
  "f2": "F2",
  "f3": "F3",
  "f4": "F4",
  "f5": "F5",
  "f6": "F6",
  "f7": "F7",
  "f8": "F8",
  "f9": "F9",
  "f10": "F10",
  "f11": "F11",
  "f12": "F12",
  "numLock": "NumLock",
  "scrollLock": "ScrollLock",
  "semiColon": "セミコロン",
  "equalSign": "等号",
  "comma": "コンマ",
  "dash": "ダッシュ",
  "period": "ピリオド",
  "forwardSlash": "スラッシュ",
  "graveAccent": "重アクセント",
  "openBracket": "左かっこ",
  "backSlash": "バックスラッシュ",
  "closeBracket": "右かっこ",
  "singleQuote": "単一引用符"
});
