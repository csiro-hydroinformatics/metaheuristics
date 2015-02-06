set R="c:\Program Files\R\R-3.1.2\bin\x64\R.exe"
rm mh_0.*.tar.gz
%R% CMD build mh
REM %R% CMD check mh_0.7-3.tar.gz
%R% CMD INSTALL mh_0.*.tar.gz
