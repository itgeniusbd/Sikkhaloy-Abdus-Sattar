cd /d %~dp0

echo system is x86
	copy .\*.dll %windir%\system32\
	
echo system is x64
	copy .\*.dll %windir%\SysWOW64\			
pause