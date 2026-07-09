import { useQuery } from "@tanstack/react-query";
import { Quote } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { religionCenterApi } from "../api/religionCenterApi";

export function DailyQuoteCard() {
  const { data: quote } = useQuery({ queryKey: ["dailyQuote"], queryFn: religionCenterApi.getDailyQuote });
  if (!quote) return null;

  return (
    <Card>
      <CardContent className="flex items-start gap-3 pt-5">
        <Quote className="mt-1 size-5 shrink-0 text-primary" />
        <div>
          <p className="italic">"{quote.text}"</p>
          {quote.source && <p className="mt-1 text-sm text-muted-foreground">— {quote.source}</p>}
        </div>
      </CardContent>
    </Card>
  );
}
