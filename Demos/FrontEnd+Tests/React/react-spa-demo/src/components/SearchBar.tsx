// This will be a "controlled component"
// It's an input element who's value is driven by react state 
// NOT the browser's default DOM behavior
// React acts as the source of truth for what the component holds/renders

interface SearchBarProps {
    value: string;
    onChange: (value: string) => void; // taking in a function as a prop!
}

export function SearchBar ({value, onChange}: SearchBarProps) {
    return (
        <input 
            type="search" 
            placeholder="Filter by name..."
            value={value} // Value passed in from prop        
            onChange={(e) => onChange(e.target.value)}
        />
    );
}