mkdir patch
candle.exe Patch.wxs -o patch\Patch.wixobj
light.exe patch\Patch.wixobj -out patch\Patch.pcp
msimsp.exe -s patch\Patch.pcp -p patch\Patch.msp -l patch\Patch.log
