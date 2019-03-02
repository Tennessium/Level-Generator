import pygame
from random import randint

pygame.init()
data = []

with open('data.txt') as f:
    while True:
        s = f.readline()
        if s == '\n':
            break

        data.append(list(map(int, (s[:-1].split()))))

k = 5
color_count = 290
colors = []

size = width, height = len(data[0]) * k, len(data) * k
screen = pygame.display.set_mode(size)
colors.append((255, 255, 255))
colors.append((150, 150, 150))
for i in range(color_count):
    kek = (randint(100, 255), randint(100, 255), randint(100, 255))
    while kek in colors:
        kek = (randint(100, 255), randint(100, 255), randint(100, 255))
    colors.append(kek)

while True:
    screen.fill((0, 0, 0))
    for i in range(0, len(data[0])):
        for j in range(0, len(data)):
            if data[j][i] == -1:
                pygame.draw.rect(screen, (0, 0, 0), (i * k, j * k, k, k))
            else:
                pygame.draw.rect(screen, (colors[data[j][i]]), (i * k, j * k, k, k))
    pygame.display.flip()
