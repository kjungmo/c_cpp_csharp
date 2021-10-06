#-*- coding:utf-8 -*-
import re
file1 = open('FineLocalizerForm2.cs', 'r', encoding='utf-8')
appliedCode = ''
while True:
    line = file1.read()
    if not line:
        break
    line = re.sub('"!@(\w+)"', r'Lang.Msg.\1', line, flags=re.MULTILINE)
    appliedCode = line
    #print('Line {}\n:\n{}'.format(count, line))
print('****************')
print('appliedCode HERE')
print(appliedCode)
file1 = open('FineLocalizerForm2.cs','w+',encoding='utf-8')
file1.write(appliedCode)
file1.close()
print("regex applied\n")
