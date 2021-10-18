import sys
from cs2xlsx import create_tokens_for_localization
from cs2xlsx import add_new_tokens_to_xlsx
from xl2resx import convert_xlsx_to_resx

if __name__ == '__main__':
    if len(sys.argv) == 3:
        match = create_tokens_for_localization(sys.argv[1])
        if match:
            add_new_tokens_to_xlsx(sys.argv[2], match)
        convert_xlsx_to_resx(sys.argv[2])
    elif len(sys.argv) == 2:
        convert_xlsx_to_resx(sys.argv[1])
    else:
        print("\nUsage : python3 cs2xl2resx.py [ CS_FOLDERNAME ] FILENAME.xlsx")
    
        
