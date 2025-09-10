@echo off

del /Q "..\LCH_RTS_BOT_TEST\FlatBuffers\packet_generated.*"
del /Q "..\LCH_RTS_GAME\FlatBuffers\packet_generated.*"
del /Q "..\LCH_RTS_MATCHING\FlatBuffers\packet_generated.*"
del /Q "..\LCH_RTS_CLIENT\Assets\Scripts\Packet\packet_generated.*

for /r %%f in (*.fbs) do (
	flatc --csharp --gen-onefile -o ../LCH_RTS_BOT_TEST/FlatBuffers/ "%%f"
	flatc --csharp --gen-onefile -o ../LCH_RTS_GAME/FlatBuffers/ "%%f"
	flatc --csharp --gen-onefile -o ../LCH_RTS_MATCHING/FlatBuffers/ "%%f"
	flatc --csharp --gen-onefile -o ../LCH_RTS_CLIENT/Assets/Scripts/Packet/ "%%f"
)