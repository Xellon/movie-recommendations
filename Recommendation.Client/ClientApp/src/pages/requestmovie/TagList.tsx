import * as React from "react";
import { Chip } from "@material-ui/core";
import * as DB from "../../model/DB";

export interface TagListProps {
  tags: DB.Tag[];
  onTagSelected:
  (tagIds: number[]) => void;
}
export function TagList(props: TagListProps) {
  const [selectedTags, setSelectedTags] = React.useState<number[]>([]);
  const onTagClick = (id: number) => {
    const tagId = selectedTags.find(t => t === id);
    const tagIds = tagId ? selectedTags.filter(v => v !== id) : selectedTags.concat([id]);
    props.onTagSelected(tagIds);
    setSelectedTags(tagIds);
  };
  return (
    <>
      {props.tags.map(tag => (
        <Chip
          key={tag.id}
          className={"requestmovie-chip" + (selectedTags.find(t => t === tag.id) ? " selected" : "")}
          label={tag.text}
          variant="outlined"
          // tslint:disable-next-line:jsx-no-lambda
          onClick={() => onTagClick(tag.id)}
        />))}
    </>
  );
}
