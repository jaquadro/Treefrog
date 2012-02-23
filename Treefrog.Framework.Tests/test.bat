call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
call msbuild
call "C:\Program Files (x86)\NUnit 2.5.10\bin\net-2.0\nunit-x86.exe" /run %1