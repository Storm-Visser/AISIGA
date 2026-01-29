def replace_commas(filename):
    with open(filename, 'r', encoding='latin1') as file:
        content = file.read()

    # Replace "," with "." only if not followed by a space
    import re
    updated_content = re.sub(r',(?! )', '.', content)

    with open(filename, 'w', encoding='latin1') as file:
        file.write(updated_content)


replace_commas('../MAIMBASELINE/MAIMBASELINE.txt')
