import re

def AddArraysToResults(filepath):
    with open(filepath, 'r', encoding='latin1') as file:
        lines = file.readlines()

    updated_lines = []
    current_dataset = None
    test_accs = []
    times = []
    dataset_block = []  # buffer lines for the current dataset block

    def flush_dataset_block():
        # flush the dataset block with appended TestAcc and Time arrays at the end
        if dataset_block:
            # append TestAcc and Time arrays if available
            if test_accs and times:
                dataset_block.append(f"TestAcc = {test_accs}\n")
                dataset_block.append(f"Time = {times}\n")
            updated_lines.extend(dataset_block)

    for i, line in enumerate(lines):
        header_match = re.match(r'^(\w+):\s*$', line.strip())

        if header_match:
            # flush the previous dataset block before starting new one
            flush_dataset_block()

            # reset for new dataset
            dataset_block = [line]  # start new block with header line
            current_dataset = header_match.group(1)
            test_accs = []
            times = []

        else:
            # Accumulate test acc and times for fold lines
            if "Fold" in line:
                test_match = re.search(r'Test Acc\s*=\s*([\d.]+)', line)
                time_match = re.search(r'Time\s*=\s*([\d.]+)s', line)
                if test_match and time_match:
                    test_accs.append(float(test_match.group(1)))
                    times.append(float(time_match.group(1)))

            dataset_block.append(line)

    # flush the last dataset block
    flush_dataset_block()

    # write back to file
    with open(filepath, 'w', encoding='latin1') as file:
        file.writelines(updated_lines)

# Run the function
AddArraysToResults('../Phase1/Exp1_0Base.txt')
AddArraysToResults('../Phase1/Exp1_1.txt')
AddArraysToResults('../Phase1/Exp1_2.txt')
AddArraysToResults('../Phase1/Exp1_3.txt')

AddArraysToResults('../Phase2/Exp2_1.txt')
AddArraysToResults('../Phase2/Exp2_2.txt')
AddArraysToResults('../Phase2/Exp2_3.txt')
AddArraysToResults('../Phase2/Exp2_4.txt')

AddArraysToResults('../Phase3/Exp3_1NEW.txt')
AddArraysToResults('../Phase3/Exp3_2NEW.txt')

AddArraysToResults('../Phase4/Exp4_0.txt')
AddArraysToResults('../Phase4/Exp4_1.txt')
AddArraysToResults('../Phase4/Exp4_2.txt')
