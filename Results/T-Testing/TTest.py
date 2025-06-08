from scipy.stats import ttest_rel

TestAcc = [54.89, 52.5, 55.81, 56.19, 57.25]
Time = [0.77, 0.77, 0.79, 0.78, 0.78]
TestAcc1 = TestAcc
Time1 = Time
TestAcc = [52.78, 51.93, 58.49, 54.64, 52.5]
Time = [0.81, 0.77, 0.78, 0.77, 0.79]

t_stat1, p_value1 = ttest_rel(TestAcc, TestAcc1)
t_stat2, p_value2 = ttest_rel(Time, Time1)

def format_p(p):
    return f"{p:.4f}" if p >= 1e-4 else f"{p:.2e}"

print(f"Accuracy: t = {t_stat1:.4f}, p = {format_p(p_value1)} | Time: t = {t_stat2:.4f}, p = {format_p(p_value2)}")

