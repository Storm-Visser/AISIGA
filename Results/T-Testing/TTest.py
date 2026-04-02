from scipy.stats import ttest_rel

TestAcc = [46.67, 49.89, 51.63, 51.67, 50.62]
Time = [4.56, 4.58, 4.64, 4.65, 4.79]
TestAcc1 = TestAcc
Time1 = Time
TestAcc = [48.89, 51.93, 50.81, 50.48, 54.0]
Time = [0.43, 0.4, 0.41, 0.4, 0.41]

t_stat1, p_value1 = ttest_rel(TestAcc, TestAcc1)
t_stat2, p_value2 = ttest_rel(Time, Time1)

def format_p(p):
    return f"{p:.4f}" if p >= 1e-4 else f"{p:.2e}"

print(f"Accuracy: t = {t_stat1:.4f}, p = {format_p(p_value1)} | Time: t = {t_stat2:.4f}, p = {format_p(p_value2)}")

