#-*- coding:utf-8 -*-
import re
import os
import sys

def tokenize_localization_key_by_regex(filename):
    if not os.path.exists(filename):
        print('no .cs file')
        return

    file = open(filename, 'r', encoding='utf-8')
    appliedCode = ''
    while True:
        line = file.read()
        if not line:
            break
        line = re.sub('"!@(\w+)"', r'Lang.Msg.\1', line, flags=re.MULTILINE)
        appliedCode = line
    file.close()
    file = open(filename, 'w+', encoding='utf-8')
    file.write(appliedCode)
    file.close()
    print("regex applied\n")

if __name__ == '__main__':
    if len(sys.argv) > 1:
        tokenize_localization_key_by_regex(sys.argv[1])
    else:
        print("Usage : python3 CSharpener.py filename.cs")
