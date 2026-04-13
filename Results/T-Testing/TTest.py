from scipy.stats import ttest_rel

TestAcc = [64.94, 64.94, 64.94, 65.36, 65.36]
Time = [16.35, 16.37, 16.25, 16.26, 16.23]
TestAcc1 = TestAcc
Time1 = Time
TestAcc = [64.94, 64.94, 64.94, 65.36, 65.36]
Time = [607.89, 601.26, 607.45, 602.53, 600.29]

t_stat1, p_value1 = ttest_rel(TestAcc, TestAcc1)
t_stat2, p_value2 = ttest_rel(Time, Time1)

def format_p(p):
    return f"{p:.4f}" if p >= 1e-4 else f"{p:.2e}"

print(f"Accuracy: t = {t_stat1:.4f}, p = {format_p(p_value1)} | Time: t = {t_stat2:.4f}, p = {format_p(p_value2)}")

