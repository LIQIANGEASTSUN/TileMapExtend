
Vector3 center = grid.CellToWorld(cell.Position);
float w = sprite.bounds.size.x;
float h = sprite.bounds.size.y;
_vertices[vStart + 0] = center + new Vector3(-w / 2, -h / 2);
_vertices[vStart + 1] = center + new Vector3(-w / 2, h / 2);
_vertices[vStart + 2] = center + new Vector3(w / 2, h / 2);
_vertices[vStart + 3] = center + new Vector3(w / 2, -h / 2);

var rect = sprite.textureRect;
var tex = sprite.texture;
Vector2 uv00 = new Vector2(rect.xMin / tex.width, rect.yMin / tex.height);
Vector2 uv11 = new Vector2(rect.xMax / tex.width, rect.yMax / tex.height);
_uvs[vStart + 0] = new Vector2(uv00.x, uv00.y);
_uvs[vStart + 1] = new Vector2(uv00.x, uv11.y);
_uvs[vStart + 2] = new Vector2(uv11.x, uv11.y);
_uvs[vStart + 3] = new Vector2(uv11.x, uv00.y);

_triangles[tStart + 0] = vStart + 0;
_triangles[tStart + 1] = vStart + 1;
_triangles[tStart + 2] = vStart + 2;
_triangles[tStart + 3] = vStart + 2;
_triangles[tStart + 4] = vStart + 3;
_triangles[tStart + 5] = vStart + 0;