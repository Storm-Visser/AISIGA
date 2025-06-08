algo1 = [0.845, 0.855, 0.841, 0.850, 0.848]
algo2 = [0.823, 0.832, 0.828, 0.833, 0.829]

# Compute differences
differences = [a1 - a2 for a1, a2 in zip(algo1, algo2)]

# Compute mean and std of the differences
mean_diff = sum(differences) / len(differences)
std_diff = (sum((d - mean_diff)**2 for d in differences) / (len(differences) - 1)) ** 0.5

# Compute t
from math import sqrt
t_stat = mean_diff / (std_diff / sqrt(len(differences)))

print(f"t = {t_stat:.4f}")
