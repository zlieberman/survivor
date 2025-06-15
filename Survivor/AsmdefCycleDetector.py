import json
import os
from collections import defaultdict
from typing import Dict, List, Set

def find_asmdef_files(root_dir: str) -> List[str]:
    """Find all .asmdef files in the given directory and its subdirectories."""
    asmdef_files = []
    for root, _, files in os.walk(root_dir):
        for file in files:
            if file.endswith('.asmdef'):
                asmdef_files.append(os.path.join(root, file))
    return asmdef_files

def parse_asmdef(file_path: str) -> Dict:
    """Parse an .asmdef file and return its contents as a dictionary."""
    with open(file_path, 'r') as f:
        return json.load(f)

def build_dependency_graph(asmdef_files: List[str]) -> Dict[str, Set[str]]:
    """Build a dependency graph from .asmdef files."""
    graph = defaultdict(set)
    for file_path in asmdef_files:
        asmdef = parse_asmdef(file_path)
        assembly_name = asmdef['name']
        references = asmdef.get('references', [])
        graph[assembly_name].update(references)
    return dict(graph)

def find_cycles(graph: Dict[str, Set[str]]) -> List[List[str]]:
    """Find all cycles in the dependency graph."""
    def dfs(node: str, visited: Set[str], path: List[str], cycles: List[List[str]]):
        if node in path:
            cycle_start = path.index(node)
            cycles.append(path[cycle_start:] + [node])
            return
        if node in visited:
            return
        visited.add(node)
        path.append(node)
        for neighbor in graph.get(node, set()):
            dfs(neighbor, visited, path, cycles)
        path.pop()

    cycles = []
    visited = set()
    for node in graph:
        if node not in visited:
            dfs(node, visited, [], cycles)
    return cycles

def main():
    # Find all .asmdef files in the Assets/Scripts directory
    root_dir = os.path.join('Assets', 'Scripts')
    asmdef_files = find_asmdef_files(root_dir)
    
    # Build dependency graph
    graph = build_dependency_graph(asmdef_files)
    
    # Find cycles
    cycles = find_cycles(graph)
    
    # Print results
    print("\nDependency Graph:")
    for assembly, deps in graph.items():
        print(f"{assembly} -> {', '.join(deps)}")
    
    print("\nCycles Found:")
    if cycles:
        for cycle in cycles:
            print(" -> ".join(cycle))
    else:
        print("No cycles found!")

if __name__ == "__main__":
    main() 