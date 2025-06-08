def replace_commas(filename):
    with open(filename, 'r', encoding='latin1') as file:
        content = file.read()

    # Replace "," with "." only if not followed by a space
    import re
    updated_content = re.sub(r',(?! )', '.', content)

    with open(filename, 'w', encoding='latin1') as file:
        file.write(updated_content)


replace_commas('../Phase1/Exp1_1.txt')
replace_commas('../Phase1/Exp1_2.txt')
replace_commas('../Phase1/Exp1_3.txt')

replace_commas('../Phase2/Exp2_1.txt')
replace_commas('../Phase2/Exp2_2.txt')
replace_commas('../Phase2/Exp2_3.txt')
replace_commas('../Phase2/Exp2_4.txt')

replace_commas('../Phase3/Exp3_1NEW.txt')
replace_commas('../Phase3/Exp3_2NEW.txt')

replace_commas('../Phase4/Exp4_0.txt')
replace_commas('../Phase4/Exp4_1.txt')
replace_commas('../Phase4/Exp4_2.txt')
