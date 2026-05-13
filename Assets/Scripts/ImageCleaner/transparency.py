from PIL import Image
from collections import deque

input_file = "kael_smiling.png"
output_file = "SmilingKael.png"

img = Image.open(input_file).convert("RGBA")
pixels = img.load()

width, height = img.size

# Adjust this if background is not perfectly white
TOLERANCE = 20

def is_white(pixel):
    r, g, b, a = pixel
    return (
        a > 0 and
        r >= 255 - TOLERANCE and
        g >= 255 - TOLERANCE and
        b >= 255 - TOLERANCE
    )

visited = set()
queue = deque()

# Add all border pixels to the queue if they are white
for x in range(width):
    if is_white(pixels[x, 0]):
        queue.append((x, 0))
    if is_white(pixels[x, height - 1]):
        queue.append((x, height - 1))

for y in range(height):
    if is_white(pixels[0, y]):
        queue.append((0, y))
    if is_white(pixels[width - 1, y]):
        queue.append((width - 1, y))

# Flood-fill only the white background connected to the edges
while queue:
    x, y = queue.popleft()

    if (x, y) in visited:
        continue

    visited.add((x, y))

    if not is_white(pixels[x, y]):
        continue

    # Make background pixel transparent
    r, g, b, a = pixels[x, y]
    pixels[x, y] = (r, g, b, 0)

    # Check neighboring pixels
    for nx, ny in [
        (x + 1, y),
        (x - 1, y),
        (x, y + 1),
        (x, y - 1)
    ]:
        if 0 <= nx < width and 0 <= ny < height:
            if (nx, ny) not in visited and is_white(pixels[nx, ny]):
                queue.append((nx, ny))

img.save(output_file)