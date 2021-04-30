# FEGBA_Mess_Reloc
Fire Emblem GBA Message Relocator

A short code written in C# that relocates encoded strings in GBA Fire Emblem games.

Currently, this supports ones based on one of the following:
* Fire Emblem - Fuuin no Tsurugi (Japan) (The Binding Blade, ``AFEJ``, ``FE6J``)
* Fire Emblem - Rekka no Ken (Japan) (``AE7J``, ``FE7J``)
* Fire Emblem (USA, Australia) (The Blazing Blade, ``AE7E``, ``FE7U``)
* Fire Emblem - Seima no Kouseki (Japan) (``BE8J``, ``FE8J``)
* Fire Emblem - The Sacred Stones (USA, Australia) (``BE8E``, ``FE8U``)

This doesn't support ones that the anti-Huffman patch is applied, since the unencoded strings are likely not fit to the original location.
Because of a similar reason, even if the anti-Huffman patch is not applied, this aborts the job if the total length of encoded strings exceeds the capacity.

Even if the original location for strings are occupied by other data, like graphics or fonts, this currently ignores and overwrites them.

The output file name will be always ``{name}_Rewrite.{ext}``, so don't worry for the input work to be overwritten.

## How this works
Get the Huffman table from the ROM (because boundaries between characters cannot be found without it), then copy encoded strings, then zero-fill where the strings were, and then relocate strings from the start of the original string location.
