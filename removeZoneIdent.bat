rem @echo off
echo ファイルのZone識別子を削除します。

cd /d "%~dp0"
rem if exist ".\Remcoposer64.exe" ( 

	echo on

	DIR /B /S > dir.txt
	FOR /f %%a in (dir.txt) do (
	echo . > %%a:Zone.Identifier
	)
	DEL dir.txt

	@echo off
	echo 完了しました。
	echo on
	pause
	exit
rem ) else (
rem 	echo Remcoposer64.exeが存在しないフォルダで実行されています。
rem 	echo 実行を中止します。
rem 	pause
rem 	echo on
rem 	exit
rem )
