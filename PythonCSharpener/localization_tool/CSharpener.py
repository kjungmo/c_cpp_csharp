#-*- coding:utf-8 -*-
import re
import os
import sys
import glob
from openpyxl import load_workbook

def create_tokens_for_localization(cs_filename):
    match = []
    applied_cs_code = ''
    with open(cs_filename, 'r', encoding='utf-8') as file:
        while True:
            line = file.read()
            if not line:
                break

            tokens = re.findall('"!@(\w+)"', ''.join(line))

            for items in tokens:
                match.append(items)
                match = list(dict.fromkeys(match))

            line = re.sub('"!@(\w+)"', r'Lang.Msgs.\1', line, flags=re.MULTILINE)
            applied_cs_code = line

    with open(cs_filename, 'w', encoding='utf-8') as file:
        file.write(applied_cs_code)
        file.close()
          
    return match

def add_new_tokens_to_xlsx(xlsx_filename, list_of_tokens):
    if not os.path.exists(xlsx_filename):
        print('no .xlsx file')
        return

    workbook = load_workbook(xlsx_filename)
    worksheet = workbook["Msgs"]

    tokens_from_xlsx = []
    col_a = worksheet['A']
    for cell in col_a:
        tokens_from_xlsx.append(cell.value)
        
    print("\ntokens from excel : ")
    print(tokens_from_xlsx)
    print("\nlist of tokens : ")
    print(list_of_tokens)
    
    tokens_to_be_added = list(set(list_of_tokens) - set(tokens_from_xlsx))
    print("\ntokens to be added : ")
    print(tokens_to_be_added)

    for i in range(len(tokens_to_be_added)):
        worksheet.cell(len(tokens_from_xlsx) + 1 + i, 1, value = tokens_to_be_added[i]) # row , col 

    workbook.save(xlsx_filename)
    
#     workbook = load_workbook(xlsx_filename)
#     worksheet = workbook["Msgs"]
#     tokens_from_xlsx_after = []
#     col_a_after = worksheet['A']
#     for cell in col_a:
#         tokens_from_xlsx_after.append(cell.value)
        
#     print("\ntokens from excel afterwards : ")
#     print(tokens_from_xlsx_after)
    

if __name__ == '__main__':
    if len(sys.argv) > 1:
        if os.path.isdir("./PythonCSharpener/" + sys.argv[1]):
            match = []
            for cs_file in glob.glob(os.path.abspath("./PythonCSharpener" + sys.argv[1] + "/*.cs")):
                for item in create_tokens_for_localization(cs_file):
                    match.append(item)

            match = list(set(match))
            print("\nregex applied\n")
            print(match)
            add_new_tokens_to_xlsx("./PythonCSharpener/localization_tool/" + sys.argv[2], match)
            
        else:
            print('no folder')
        
    else:
        print("Usage : python3 CSharpener.py CS_FOLDERNAME FILENAME.xlsx")