# BSModManager
ModAssistantで管理できないBeat SaberのModを管理するツール

## 実行環境
.Net Frameworkの4.8が必要です。

## 導入方法
Setup.zipを[Releases](https://github.com/rakkyo150/BSModManager/releases)からダウンロードして中にあるSetup.msiを起動してインストールしてください。<br>
もしインストールできなかったらBSModManager.zipも試してみてください。<br>
なお、アップデートは起動時に自動的に確認するので、基本的にアップデート作業は手動で行う必要はないはずです。<br>

## 使い方
GitHubのPersonal access Tokenが必要です。<br>
GitHubのPersonal access tokenは、GitHubのアカウントを作成の上、Settings->Developer settings->Personal access tokensから生成してください。<br>
Select Scopesは設定しなくて大丈夫です。<br>

また、起動する前にModAssistantでModを入れて一度Beat Saberを起動しておくと安心です。
そうでない場合のテストはしてないので。

起動はBSModManager.exeやインストーラーで生成されたショートカットから行ってください。<br>
初期設定ダイアログに従って初期設定をしてください。

### 初期設定ダイアログ

![初期設定画面](https://user-images.githubusercontent.com/86054813/170963831-e62d6f9b-e1d9-4c02-aaeb-d6a1e4b28b98.png)

GitHub Tokenには、Personal access tokenでコピーしたものを入力してVerifyボタンを押してください。<br>
すべて入力し終わって〇が付いたら、下のFinishボタンが押せるようになるので、それを押して初期設定は終了です。

初期設定が終わったらインストールしているModが読み込まれます。<br>
PluginsフォルダやIPA/Pending/Plugingフォルダのdllファイルは読み込まれますが、その他は読み込まれないので注意です。<br>
読み込みが終わったらModの情報の変更や追加を行いましょう。<br>

### Update画面

初期設定が終わったら、インストールしているModの読み込みなどが行われます。<br>
PluginsフォルダやIPA/Pending/Plugingフォルダのdllファイルは読み込まれますが、その他は読み込まれないので注意です。<br>
読み込みが終わったらアップデート画面の操作が可能になります。

![アップデート画面](https://user-images.githubusercontent.com/86054813/170963389-e70e6757-a9f7-4b73-a375-c64ede837aa0.png)

以前Beat Saberのバージョンでこのツールで管理していたModがある場合はその情報を、無い場合はRecommend Mod(後述)の情報が入力されているはずです。<br>
URL情報からModをダウンロード不可能な場合、青でInnstalleとUpdatedが表示されLatestのバージョンは0.0.0になります。<br>
URL情報が設定されていない場合、UpdatedやDescriptionが?になります。<br>
URL情報が設定されている場合は、UpdatedやDescriptionは---になります。<br>
URL情報からModをダウンロード可能な場合、LatestがInstalledより高い場合赤で、同じ場合は緑で、低い場合はオレンジでInstalledとUpdatedが表示されます。<br>
オレンジの場合はURL情報やOriginalが間違っている可能性があるので確認してみましょう。

情報が追加したり変更したい場合は、Mod情報を入力してください。<br>
対象のModにチェックを付けてデータ変更ボタン(下のボタンの右から４番目)を押してください。<br>
すると、[Mod情報変更ダイアログ](#Mod情報変更ダイアログ)が出てきます。

また、アップデートするにはAll Check/UncheckボタンですべてのModにチェックしてUpdateボタンを押すだけでOKです<br>
ローカルのバージョン情報はメタデータを参照しているので、メタデータの更新を忘れているModの場合はアップデートが成功していても画面上のバージョンは変わらなかったりします。<br>
ModAssistantで管理できるModに関しては、更新バージョンがありModAssistantのパスが設定されている場合、ModAssistantが開くのでそこでアップデートしてください。<br>
GitHubのリポジトリのURLが設定されていなどの場合は、対象のModにチェックを付けて、Open URLボタンを押してURLに飛んで手動でアップデートの確認をしてください。<br>
ModAssistantでアップデートしたり手動アップデートをした場合、Refreshボタン(下の右から５番目のボタン)を押すと反映されます。<br>

### Mod情報変更ダイアログ

![Mod情報変更ダイアログ](https://user-images.githubusercontent.com/86054813/170966730-6fc22075-d62d-4f35-a09c-09e4a3e4d07c.png)

Originalはチェックを付けると、同名のModがModAssistantに登録されている場合はModAssistant中心で管理するようになります。<br>
改造版のModなどはチェックを外しておきましょう。<br>

URL欄にはModのGitHubのリポジトリのURLを入力してください。<br>
もしGitHubのリポジトリが無い場合は、Mod情報が載っているURLを入力するといいでしょう。<br>
SearchボタンでMod名でGoogle検索を行えます。<br>

次のModの情報を入力する場合はNextボタン、前のModの情報入力を修正する場合はBackボタン、Mod情報入力を途中でやめる場合はExitボタンを押してください。

### インストール画面
![インストール画面](https://user-images.githubusercontent.com/86054813/170969879-27f303cf-6705-41f6-b5ea-af00a6657c29.png)

インストール画面にはPast DataタブとRecommendタブがあります。<br>
Past Dataタブには以前のBeat SaberのバージョンでインストールしていたModでこのツールで管理していたModをアップデートと同じ要領でインストールすることができます。<br>
既にインストールされている場合は、そのModはここには表示されません。<br>
一応Mod情報を変更できますが、次回以降の実行に持ちこすことはできないので注意です。<br>

Recommendタブには、僕が独断と偏見でよく使われてたり面白いModでModAssistantでみかけないものを選んだので、それをアップデートと同じ要領でインストールできます。<br>
既にインストールされている場合は、そのModはここには表示されません。<br>
Recommendに追加するModの選定に関しては、僕ひとりでやるのではなく、管理するグループを作るといいかなとは思ってます。<br>
いい案募集中

### 設定画面
初期設定で設定した設定を変更したり、ログフォルダやデータフォルダ、バックアップフォルダ、一時フォルダを開けます。<br>
データフォルダにはMod情報のデータがあります。<br>
バックアップフォルダにはPluginsフォルダやデータフォルダ、config.json(初期設定で設定した設定のファイル)があります。<br>
もしデータを誤って消してしまった場合は助かるでしょう。
