#-*- coding:utf-8 -*-
import re
import os
import sys
import glob
from openpyxl import load_workbook

def create_tokens_for_localization(project_directory):

    if not os.path.isdir("../" + project_directory):
        print('no folder')
        return
    
    switch = False
    match = []

    for cs_file in glob.glob(os.path.abspath("../" + project_directory + "/*.cs")):
        applied_cs_code = ''
        with open(cs_file, 'r', encoding='utf-8') as file:
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

        with open(cs_file, 'w', encoding='utf-8') as file:
            file.write(applied_cs_code)
            file.close()
          
    print("regex applied\n")
    print(match)
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

    tokens_to_be_added = [x for x in list_of_tokens if x not in list(set(tokens_from_xlsx))]

    for i in range(len(tokens_to_be_added)):
        worksheet.cell(len(tokens_from_xlsx) + 1 + i, 1, value = tokens_to_be_added[i]) # row , col 

    workbook.save(xlsx_filename)

if __name__ == '__main__':
    if len(sys.argv) > 1:
        match = create_tokens_for_localization(sys.argv[1])
        add_new_tokens_to_xlsx(sys.argv[2], match)
    else:
        print("Usage : python3 CSharpener.py cs_filename.cs xlsx_filename.xlsx")